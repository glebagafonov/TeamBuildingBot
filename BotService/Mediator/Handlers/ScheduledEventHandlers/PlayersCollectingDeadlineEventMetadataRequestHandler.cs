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
using BotService.Mediator.Requests;
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
        PlayersCollectingDeadlineEventMetadataRequestHandler : IRequestHandler<
            ScheduledEventRequest<PlayersCollectingDeadlineEventMetadata>>
    {
        private readonly IMediator _mediator;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IGameRepository _gameRepository;
        private readonly IScheduler _scheduler;
        private readonly IUserInteractionService _userInteractionService;

        public PlayersCollectingDeadlineEventMetadataRequestHandler(
            IMediator mediator,
            IServiceConfiguration serviceConfiguration,
            IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository,
            IScheduler scheduler,
            IUserInteractionService userInteractionService)
        {
            _mediator                     = mediator;
            _serviceConfiguration         = serviceConfiguration;
            _threadContextSessionProvider = threadContextSessionProvider;
            _gameRepository               = gameRepository;
            _scheduler                    = scheduler;
            _userInteractionService       = userInteractionService;
        }

        public Task<Unit> Handle(ScheduledEventRequest<PlayersCollectingDeadlineEventMetadata> message,
            CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(message.MetaData.GameId);

                if (game.AcceptedPlayers.Count != _serviceConfiguration.PlayersPerTeam * 2)
                {
                    _mediator.Send(new CancelGameRequest() {GameId = game.Id}, cancellationToken);
                }
                else
                {
                    _mediator.Send(
                        new ScheduledEventRequest<PlayersCollectingDeadlineEventMetadata>(
                            new PlayersCollectingDeadlineEventMetadata() {GameId = game.Id}), cancellationToken);
                }

                _gameRepository.Save(game);
            }

            return Task.FromResult(Unit.Value);
        }


        private void NotifyAboutSuccessfulCollecting(Game game, ICollection<TeamPlayer> distributedPlayers,
            List<BotUser> administrators)
        {
            var message = $"Игра {game.DateTime.ToString(DateTimeHelper.DateFormat)}.\n";
            foreach (var team in distributedPlayers.GroupBy(x => x.TeamNumber))
            {
                message += $"\nКоманда {team.First().TeamNumber}\n";
                foreach (var x in team.Select((value, i) => new {i, value}))
                    message +=
                        $"{(x.i + 1).ToString()}. {x.value.Player.User.FirstName} {x.value.Player.User.LastName}";
                message += $"\n";
            }

            administrators.ForEach(x => _userInteractionService.SendMessage(message, x));
            _scheduler.DeleteEvent<IGameScheduledEventMetadata>(x => x.GameId == game.Id);
        }
    }
}