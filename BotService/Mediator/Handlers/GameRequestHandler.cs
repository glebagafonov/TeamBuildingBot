using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class GameRequestHandler : IRequestHandler<NextGameStatusRequest, string>
    {
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IGameRepository _gameRepository;

        public GameRequestHandler(
            IThreadContextSessionProvider threadContextSessionProvider, 
            IGameRepository gameRepository)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _gameRepository = gameRepository;
        }

        public Task<string> Handle(NextGameStatusRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.ListBySpecification(new UndeletedEntities<Game>())
                    .Where(x => DateTime.Now < x.DateTime && x.IsActive)
                    .OrderBy(x => x.DateTime)
                    .FirstOrDefault();
                if (game == null)
                {
                    var message = "Нет запланированной игры.";
                    return Task.FromResult(message); 
                }
                else
                {
                    var message = $"Игра {game.DateTime}\nСогласившиеся игроки:\n";
                    message += string.Join("\n",
                        game.AcceptedPlayers.Select((x, index) =>
                            $"{index + 1}. {x.User.FirstName} {x.User.LastName}"));
                    message += "\nЗапрошенные игроки:\n";
                    message += string.Join("\n",
                        game.RequestedPlayers.Select((x, index) =>
                            $"{index + 1}. {x.Player.User.FirstName} {x.Player.User.LastName}"));
                    message += "\nОтказавшиеся игроки:\n";
                    message += string.Join("\n",
                        game.RejectedPlayers.Select((x, index) =>
                            $"{index + 1}. {x.User.FirstName} {x.User.LastName}"));
                    return Task.FromResult(message); 
                }

                
            }
        }
    }
}