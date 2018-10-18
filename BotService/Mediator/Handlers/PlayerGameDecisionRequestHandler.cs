using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class PlayerGameDecisionRequestHandler : IRequestHandler<PlayerGameDecisionRequest>
    {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IScheduler _scheduler;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ICommunicatorFactory _communicatorFactory;
        private readonly IMediator _mediator;


        public PlayerGameDecisionRequestHandler(
            IServiceConfiguration serviceConfiguration,
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository,
            IGameRepository gameRepository,
            IScheduler scheduler,
            IUserInteractionService userInteractionService,
            ICommunicatorFactory communicatorFactory,
            IMediator mediator)
        {
            _serviceConfiguration = serviceConfiguration;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository = playerRepository;
            _gameRepository = gameRepository;
            _scheduler = scheduler;
            _userInteractionService = userInteractionService;
            _communicatorFactory = communicatorFactory;
            _mediator = mediator;
        }

        public Task<Unit> Handle(PlayerGameDecisionRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var player = _playerRepository.Get(request.PlayerId);
                var game   = _gameRepository.Get(request.GameId);

                
                if (request.Decision)
                {
                    ProcessAccept(game, player);
                }
                else
                {
                    ProcessReject(game, player);
                }
                _gameRepository.Save(game);
            }
            return Task.FromResult(Unit.Value);
        }

        private void ProcessAccept(Game game, Player player)
        {
            game.RequestedPlayers.RemoveAll(x => x.Player.Id == player.Id);
            game.AcceptedPlayers.Add(player);
            if (game.AcceptedPlayers.Count == _serviceConfiguration.PlayersPerTeam * 2)
            {
                _scheduler.DeleteEvent<PlayersCollectedEventMetadata>(x => x.GameId == game.Id);
                _mediator.Send(
                    new ScheduledEventRequest<PlayersCollectedEventMetadata>(new PlayersCollectedEventMetadata()
                                                                             {GameId = game.Id}));
            }
        }

        private void ProcessReject(Game game, Player player)
        {
            AddPlayerToDeclinedPlayersList(game, player);
            ProcessNewPlayer(game);
        }
        
        private void ProcessNewPlayer(Game game)
        {
            var nextPlayer = game.SortedPlayersByRating
                .Where(x => game.RequestedPlayers.All(y => x.Player.Id != y.Player.Id))
                .Where(x => game.AcceptedPlayers.All(y => x.Player.Id != y.Id))
                .Where(x => game.DeclinedPlayers.All(y => x.Player.Id != y.Id))
                .OrderBy(x => x.OrderNumber)
                .FirstOrDefault();
            if (nextPlayer != null)
            {
                var timeoutTime = DateTime.Now.Add(_serviceConfiguration.InviteTime, _serviceConfiguration.StartDayTime,
                    _serviceConfiguration.EndDayTime);
                var playerGameAcceptanceTimeoutEventMetadata = new PlayerGameAcceptanceTimeoutEventMetadata()
                                                               {
                                                                   GameId   = game.Id,
                                                                   PlayerId = nextPlayer.Player.Id
                                                               };
                _scheduler.AddEvent(playerGameAcceptanceTimeoutEventMetadata, timeoutTime);
                game.RequestedPlayers.Add(new PlayerEvent() { Id = Guid.NewGuid(), EventTime = timeoutTime, Player = nextPlayer.Player});
                
                _userInteractionService.StartGameConfirmationDialog(nextPlayer.Player, nextPlayer.Player.User.UserAccounts.Select(x => _communicatorFactory.GetCommunicator(x)).ToList(), game.Id);

            }
        }

        private static void AddPlayerToDeclinedPlayersList(Game game, Player player)
        {
            game.RequestedPlayers.RemoveAll(x => x.Player.Id == player.Id);
            game.DeclinedPlayers.Add(player);
        }
    }
}