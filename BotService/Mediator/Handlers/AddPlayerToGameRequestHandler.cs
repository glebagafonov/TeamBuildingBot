using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;
using NHibernate.Util;

namespace BotService.Mediator.Handlers
{
    public class AddPlayerToGameRequestHandler : IRequestHandler<AddPlayerToGameRequest>
    {
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IGameRepository _gameRepository;
        private readonly IUserInteractionService _userInteractionService;
        private readonly IPlayerRepository _playerRepository;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IScheduler _scheduler;

        public AddPlayerToGameRequestHandler(
            IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository,
            IUserInteractionService userInteractionService,
            IPlayerRepository playerRepository,
            IServiceConfiguration serviceConfiguration,
            IScheduler scheduler)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _gameRepository               = gameRepository;
            _userInteractionService       = userInteractionService;
            _playerRepository             = playerRepository;
            _serviceConfiguration         = serviceConfiguration;
            _scheduler                    = scheduler;
        }

        public Task<Unit> Handle(AddPlayerToGameRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game    = _gameRepository.Get(request.GameId);
                var players = request.PlayerIds.Select(x => _playerRepository.Get(x)).ToList();
                if (_serviceConfiguration.PlayersPerTeam * 2 - (game.AcceptedPlayers.Count + game.RequestedPlayers.Count) != players.Count())
                {
                    throw new InvalidInputException($"В игре {game.DateTime} уже достаточное количество игроков");
                }

                game.RejectedPlayers.RemoveAll(x => players.Any(y => y.Id == x.Id));
                game.RequestedPlayers.RemoveAll(x => players.Any(y => y.Id == x.Id));
                foreach (var player in players)
                {
                    game.AcceptedPlayers.Add(player);
                }
                _scheduler.DeleteEvent<PlayerGameAcceptanceTimeoutEventMetadata>(x =>
                    x.GameId == game.Id && players.Any(y => y.Id == x.PlayerId));
                NotifyPlayers(players, game);
                _gameRepository.Save(game);
            }

            return Task.FromResult(Unit.Value);
        }

        private void NotifyPlayers(List<Player> players, Game game)
        {
            var message =
                $"Ты добавлен в игру {game.DateTime.ToString(DateTimeHelper.DateFormat)}";
            foreach (var player in players)
            {
                _userInteractionService.StopDialog(player.User);
                _userInteractionService.SendMessage(message, player.User);
            }
        }
    }
}