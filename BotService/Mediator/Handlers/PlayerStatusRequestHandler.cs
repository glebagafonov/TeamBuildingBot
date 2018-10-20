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
    public class PlayerStatusRequestHandler : IRequestHandler<PlayerStatusRequest, bool>
    {
        private readonly ILogger                       _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository            _botUserRepository;
        private readonly IPlayerRepository _playerRepository;

        public PlayerStatusRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository, IPlayerRepository playerRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _playerRepository = playerRepository;
        }

        public Task<bool> Handle(PlayerStatusRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                return Task.FromResult(_playerRepository.ListBySpecification(new UndeletedEntities<Player>()).First(x => x.User.Id == request.UserId).IsActive); 
            }
        }
    }
}