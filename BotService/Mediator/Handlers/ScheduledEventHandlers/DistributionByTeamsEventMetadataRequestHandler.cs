using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests.ScheduledEventRequests;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;
using NHibernate.Util;
using Telegram.Bot.Types;
using Game = Bot.Domain.Entities.Game;

namespace BotService.Mediator.Handlers.ScheduledEventHandlers
{
    public class
        DistributionByTeamsEventMetadataRequestHandler : IRequestHandler<
            ScheduledEventRequest<DistributionByTeamsEventMetadata>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IScheduler _scheduler;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ICommunicatorFactory _communicatorFactory;
        private readonly IBotUserRepository _botUserRepository;

        public DistributionByTeamsEventMetadataRequestHandler(
            IMediator mediator,
            ILogger logger,
            IServiceConfiguration serviceConfiguration,
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository,
            IGameRepository gameRepository,
            IScheduler scheduler,
            IUserInteractionService userInteractionService,
            ICommunicatorFactory communicatorFactory,
            IBotUserRepository botUserRepository)
        {
            _mediator                     = mediator;
            _logger                       = logger;
            _serviceConfiguration         = serviceConfiguration;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository             = playerRepository;
            _gameRepository               = gameRepository;
            _scheduler                    = scheduler;
            _userInteractionService       = userInteractionService;
            _communicatorFactory          = communicatorFactory;
            _botUserRepository            = botUserRepository;
        }

        public Task<Unit> Handle(ScheduledEventRequest<DistributionByTeamsEventMetadata> message,
            CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(message.MetaData.GameId);
                var administrators = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .Where(x => x.Role >= EUserRole.Moderator).ToList();

                if (game.AcceptedPlayers.Count != _serviceConfiguration.PlayersPerTeam * 2)
                {
                    NotifyAboutUnsuccessfulCollecting(game, administrators);
                }
                else
                {
                    DistributePlayersPerTeams(game);
                    NotifyAboutSuccessfulCollecting(game, game.DistributedPlayers, administrators);
                    NotifyPlayers(game.DistributedPlayers, game);
                }

                _gameRepository.Save(game);
            }

            return Task.FromResult(Unit.Value);
        }

        private void NotifyAboutUnsuccessfulCollecting(Game game, List<BotUser> administrators)
        {
            var deadlineTime = game.DateTime.Subtract(_serviceConfiguration.GameDeadline,
                _serviceConfiguration.StartDayTime,
                _serviceConfiguration.EndDayTime);

            var message =
                $"Команда не набралась({game.AcceptedPlayers.Count} из {_serviceConfiguration.PlayersPerTeam * 2}). \n" +
                "Дальнейший сбор осуществляй вручную, используя команду - /addplayertogame. " +
                $"Или отмени матч командой - /cancelgame. \nВ {deadlineTime.ToString(DateTimeHelper.DateFormat)} игра отменится автоматически";

            administrators.ForEach(x => _userInteractionService.SendMessage(message, x));
        }

        private void NotifyPlayers(ICollection<TeamPlayer> players, Game game)
        {
            foreach (var player in players)
            {
                var message = $"Играем в {game.DateTime}. Ты в команде {player.TeamNumber}";
                _userInteractionService.SendMessage(message, player.Player.User);
            }
        }

        private void NotifyAboutSuccessfulCollecting(Game game, ICollection<TeamPlayer> distributedPlayers,
            List<BotUser> administrators)
        {
            var message = $"Игра {game.DateTime.ToString(DateTimeHelper.DateFormat)}.\n";
            foreach (var team in distributedPlayers.GroupBy(x => x.TeamNumber))
            {
                message += "\n";
                foreach (var x in team.Select((value, i) => new { i, value }))
                    message += $"{(x.i + 1).ToString()}. {x.value.Player.User.FirstName} {x.value.Player.User.LastName}";
            }
            administrators.ForEach(x => _userInteractionService.SendMessage(message, x));
            _scheduler.DeleteEvent<IGameScheduledEventMetadata>(x => x.GameId == game.Id);
        }

        private void DistributePlayersPerTeams(Game game)
        {
            var teams = GameHelper.GetTeams(game.AcceptedPlayers, _serviceConfiguration.PlayersPerTeam);
            game.DistributedPlayers = teams.firstTeam
                .Select(x => new TeamPlayer() {Id = Guid.NewGuid(), Player = x, TeamNumber = 1})
                .Concat(
                    teams.secondTeam.Select(x => new TeamPlayer() {Id = Guid.NewGuid(), Player = x, TeamNumber = 2}))
                .ToList();
        }
    }
}