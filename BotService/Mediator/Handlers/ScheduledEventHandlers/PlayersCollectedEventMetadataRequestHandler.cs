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

namespace BotService.Mediator.Handlers.ScheduledEventHandlers
{
    public class
        PlayersCollectedEventMetadataRequestHandler : IRequestHandler<
            ScheduledEventRequest<PlayersCollectedEventMetadata>>
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

        public PlayersCollectedEventMetadataRequestHandler(
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
            _gameRepository = gameRepository;
            _scheduler = scheduler;
            _userInteractionService = userInteractionService;
            _communicatorFactory = communicatorFactory;
            _botUserRepository = botUserRepository;
        }

        public Task<Unit> Handle(ScheduledEventRequest<PlayersCollectedEventMetadata> message,
            CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(message.MetaData.GameId);
                DistributePlayersPerTeams(game);
                _gameRepository.Save(game);

                NotifyAdministrators(game.DistributedPlayers);
                game.DistributedPlayers.ForEach(NotifyPlayer);
            }

            return Task.FromResult(Unit.Value);
        }

        private void NotifyPlayer(TeamPlayer player)
        {
            throw new NotImplementedException();
        }

        private void NotifyAdministrators(ICollection<TeamPlayer> distributedPlayers)
        {
            var administrators = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>()).Where(x => x.Role >= EUserRole.Moderator);
            _userInteractionService.
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