using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
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

namespace BotService.Mediator.Handlers.ScheduledEventHandlers
{
    public class
        PrimaryCollectingEventMetadataRequestHandler : IRequestHandler<
            ScheduledEventRequest<PrimaryCollectingEventMetadata>>
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

        public PrimaryCollectingEventMetadataRequestHandler(
            IMediator mediator,
            ILogger logger,
            IServiceConfiguration serviceConfiguration,
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository,
            IGameRepository gameRepository,
            IScheduler scheduler,
            IUserInteractionService userInteractionService,
            ICommunicatorFactory communicatorFactory)
        {
            _mediator                     = mediator;
            _logger                       = logger;
            _serviceConfiguration         = serviceConfiguration;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository             = playerRepository;
            _gameRepository = gameRepository;
            _scheduler = scheduler;
            _userInteractionService = userInteractionService;
            _communicatorFactory = communicatorFactory;
        }

        public Task<Unit> Handle(ScheduledEventRequest<PrimaryCollectingEventMetadata> message,
            CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(message.MetaData.GameId);
                game.SortedPlayersByRating = GetSortedPlayers();
                var requestedPlayers = game.SortedPlayersByRating.Take(_serviceConfiguration.PlayersPerTeam * 2);
                foreach (var requestedPlayer in requestedPlayers)
                {
                    game.RequestedPlayers.Add(RequestPlayer(requestedPlayer, game.Id, game.DateTime));
                }
                _gameRepository.Save(game);
            }

            return Task.FromResult(Unit.Value);
        }

        private PlayerEvent RequestPlayer(SortedPlayer requestedPlayer, Guid gameId, DateTime gameDateTime)
        {
            var timeoutTime = DateTime.Now.Add(_serviceConfiguration.FirstInviteTime, _serviceConfiguration.StartDayTime,
                _serviceConfiguration.EndDayTime);
            var playerGameAcceptanceTimeoutEventMetadata = new PlayerGameAcceptanceTimeoutEventMetadata()
                                                           {
                                                               GameId   = gameId,
                                                               PlayerId = requestedPlayer.Player.Id
                                                           };
            _scheduler.AddEvent(playerGameAcceptanceTimeoutEventMetadata, timeoutTime);

            _userInteractionService.StartGameConfirmationDialog(requestedPlayer.Player, requestedPlayer.Player.User.UserAccounts.Select(x => _communicatorFactory.GetCommunicator(x)).ToList(), gameId);
            
            return new PlayerEvent() { Id = Guid.NewGuid(), EventTime = timeoutTime, Player = requestedPlayer.Player};
        }


        private List<SortedPlayer> GetSortedPlayers()
        {
            return _playerRepository
                .ListBySpecification(new UndeletedEntities<Player>())
                .Where(x => x.IsActive)
                .OrderByDescending(SortCondition)
                .Select((x, index) => new SortedPlayer() {Id = Guid.NewGuid(), OrderNumber = index, Player = x})
                .ToList();
        }

        private static int SortCondition(Player x)
        {
            return x.SkillValue * x.ParticipationRatio;
        }
    }
}