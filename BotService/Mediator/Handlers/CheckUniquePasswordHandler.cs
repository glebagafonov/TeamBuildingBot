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
    public class CheckUniquePasswordHandler : IRequestHandler<CheckUniquePassword, bool>
    {
        private readonly ILogger                       _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository            _botUserRepository;

        public CheckUniquePasswordHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<bool> Handle(CheckUniquePassword request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                return Task.FromResult(_botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .Any(x => !string.IsNullOrEmpty(x.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, x.PasswordHash)));
            }
        }
    }
}