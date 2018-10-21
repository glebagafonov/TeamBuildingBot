using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Model;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using BotService.Services.VkInteraction;
using MediatR;
using Ninject;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Game = Bot.Domain.Entities.Game;

namespace BotService
{
    public partial class BotService : ServiceBase
    {
        static List<BotUser> _users = new List<BotUser>()
        {
            new BotUser()
            {
                Id        = Guid.NewGuid(),
                FirstName = "Gleb",
                LastName  = "Agafonov",
                UserAccounts = new List<BaseAccount>()
                {
                    new VkAccount()
                    {
                        Id = Guid.NewGuid(), VkId = 123
                    },

                    new TelegramAccount()
                    {
                        Id         = Guid.NewGuid(),
                        TelegramId = 317127863
                    }
                },
                Role = EUserRole.Administrator
            }
        };

        public BotService()
        {
            CreateKernel();
            InitializeComponent();
        }

        public static void StartInConsole(string[] args)
        {
            var service = new BotService();
            service.OnStart(args);
            Console.WriteLine(@"Press any key to stop program");
            Console.Read();
            service.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            var scheduler = ServiceLocator.Get<IScheduler>();
            ServiceLocator.Get<VkInteractionService>();
            ServiceLocator.Get<TelegramInteractionService>();

            RecoverySchedulerEvents(scheduler);
        }

        private void RecoverySchedulerEvents(IScheduler scheduler)
        {
            var sessionProvider = ServiceLocator.Get<IThreadContextSessionProvider>();
            var gameRepository  = ServiceLocator.Get<IGameRepository>();
            var config          = ServiceLocator.Get<IServiceConfiguration>();
            var mediator        = ServiceLocator.Get<IMediator>();
            var userInteractionService = ServiceLocator.Get<IUserInteractionService>();
            var communicatorFactory = ServiceLocator.Get<ICommunicatorFactory>();

            using (sessionProvider.CreateSessionScope())
            {
                var games = gameRepository.ListBySpecification(new UndeletedEntities<Game>()).Where(x => x.IsActive)
                    .ToList();
                var cantResumeCollectingPlayersGames = ProcessDeadlineGames(games, config, mediator);
                games.RemoveAll(x => cantResumeCollectingPlayersGames.Any(y => y.Id == x.Id));
                
                var playerRequestsGames = games.Where(x =>
                    x.DateTime > DateTime.Now.Add(config.GameScheduleThreshold, config.StartDayTime,config.EndDayTime) &&
                    cantResumeCollectingPlayersGames.All(y => y.Id != x.Id)).ToList();
                var notifyAdminAboutStatusGames = RecoverPlayersRequests(scheduler, playerRequestsGames, config, userInteractionService, communicatorFactory);
                games.RemoveAll(x => playerRequestsGames.Any(y => y.Id == x.Id));
                
                var futureGames = ProcessFutureGames(scheduler, games, config);

                var recoverRequestGames = games.Where(x => cantResumeCollectingPlayersGames.All(y => y.Id != x.Id) &&
                                                           notifyAdminAboutStatusGames.All(y => y.Id      != x.Id) &&
                                                           futureGames.All(y => y.Id != x.Id));
            }
        }

        private static List<Game> ProcessFutureGames(IScheduler scheduler, List<Game> games, IServiceConfiguration config)
        {
            var futureGames = games.Where(x =>
                    x.DateTime > DateTime.Now.Add(config.StartDayTime, config.StartDayTime, config.EndDayTime))
                .ToList();
            foreach (var futureGame in futureGames)
            {
                scheduler.AddEvent(new PrimaryCollectingEventMetadata() {GameId = futureGame.Id},
                    futureGame.DateTime.Subtract(config.StartGameProcess, config.StartDayTime,
                        config.EndDayTime));

                scheduler.AddEvent(new DistributionByTeamsEventMetadata() {GameId = futureGame.Id},
                    futureGame.DateTime.Subtract(config.GameScheduleThreshold, config.StartDayTime,
                        config.EndDayTime));
                scheduler.AddEvent(new PlayersCollectingDeadlineEventMetadata() {GameId = futureGame.Id},
                    futureGame.DateTime.Subtract(config.GameDeadline, config.StartDayTime,
                        config.EndDayTime));
            }

            return futureGames;
        }

        private static List<Game> ProcessDeadlineGames(List<Game> games, IServiceConfiguration config, IMediator mediator)
        {
            var cantResumeCollectingPlayersGames = games.Where(x =>
                    x.DateTime > DateTime.Now.Add(config.GameDeadline, config.StartDayTime, config.EndDayTime))
                .ToList();
            foreach (var cantResumeCollectingPlayersGame in cantResumeCollectingPlayersGames)
            {
                mediator.Send(new ScheduledEventRequest<PlayersCollectingDeadlineEventMetadata>(
                    new PlayersCollectingDeadlineEventMetadata() {GameId = cantResumeCollectingPlayersGame.Id}));
            }

            return cantResumeCollectingPlayersGames;
        }

        private static List<Game> RecoverPlayersRequests(IScheduler scheduler, List<Game> games, IServiceConfiguration config,
            IUserInteractionService userInteractionService, ICommunicatorFactory communicatorFactory)
        {
            
            foreach (var playerRequestGame in games)
            {
                foreach (var requestedPlayer in playerRequestGame.RequestedPlayers)
                {
                    var playerGameAcceptanceTimeoutEventMetadata = new PlayerGameAcceptanceTimeoutEventMetadata()
                    {
                        GameId   = playerRequestGame.Id,
                        PlayerId = requestedPlayer.Player.Id
                    };
                    scheduler.AddEvent(playerGameAcceptanceTimeoutEventMetadata,
                        DateTime.Now.Add(config.InviteTime, config.StartDayTime, config.EndDayTime));

                    userInteractionService.StartGameConfirmationDialog(requestedPlayer.Player,
                        requestedPlayer.Player.User.UserAccounts.Select(x => communicatorFactory.GetCommunicator(x)).ToList(),
                        playerRequestGame.Id);
                }

                scheduler.AddEvent(new PrimaryCollectingEventMetadata() {GameId = playerRequestGame.Id},
                    playerRequestGame.DateTime.Subtract(config.StartGameProcess, config.StartDayTime,
                        config.EndDayTime));

                scheduler.AddEvent(new DistributionByTeamsEventMetadata() {GameId = playerRequestGame.Id},
                    playerRequestGame.DateTime.Subtract(config.GameScheduleThreshold, config.StartDayTime,
                        config.EndDayTime));
            }

            return playerRequestsGames;
        }

        private static void CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                RegisterServices(kernel);
                ServiceLocator.SetRoot(kernel);
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private static void RegisterServices(StandardKernel kernel)
        {
            kernel.Load(new AppNinjectModule());
        }

        protected override void OnStop()
        {
        }
    }
}