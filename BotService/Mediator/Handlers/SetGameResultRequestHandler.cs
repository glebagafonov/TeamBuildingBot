using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class SetGameResultRequestHandler : IRequestHandler<SetGameResultRequest>
    {
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository             _playerRepository;
        private readonly IGameRepository _gameRepository;

        public SetGameResultRequestHandler(
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository,
            IGameRepository gameRepository)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository             = playerRepository;
            _gameRepository = gameRepository;
        }

        public Task<Unit> Handle(SetGameResultRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(request.GameId);
                var notPlayedPlayers = _playerRepository.ListBySpecification(new UndeletedEntities<Player>())
                    .Where(x => x.IsActive && game.AcceptedPlayers.All(y => y.User.Id != x.User.Id)).ToList();
                game.ResultSet = true;
                game.IsActive = false;
                game.GoalDifference = request.GoalDifference;
                game.TeamWinnerNumber = request.TeamWinningNumber;

                foreach (var player in game.DistributedPlayers)
                {
                    if (player.TeamNumber == request.TeamWinningNumber)
                    {
                        player.Player.SkillValue += request.GoalDifference;
                    }
                    else
                    {
                        player.Player.SkillValue -= request.GoalDifference;
                    }

                    player.Player.ParticipationRatio++;
                    if (player.Player.ParticipationRatio > 20)
                    {
                        player.Player.ParticipationRatio = 20;
                    }

                    _playerRepository.Save(player.Player);
                }

                DecreaseParticipation(notPlayedPlayers);
                
                _gameRepository.Save(game);
                
            }

            return Task.FromResult(Unit.Value);
        }

        private void DecreaseParticipation(List<Player> notPlayedPlayers)
        {
            foreach (var notPlayedPlayer in notPlayedPlayers)
            {
                if (notPlayedPlayer.IsActive)
                {
                    notPlayedPlayer.ParticipationRatio--;
                    if (notPlayedPlayer.ParticipationRatio < 0)
                    {
                        notPlayedPlayer.ParticipationRatio = 0;
                    }

                    _playerRepository.Save(notPlayedPlayer);
                }
            }
        }
    }
}