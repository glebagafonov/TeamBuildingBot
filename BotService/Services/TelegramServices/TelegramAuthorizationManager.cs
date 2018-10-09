using System.Linq;
using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Model;
using BotService.Services.Interfaces;

namespace BotService.Services.TelegramServices
{
    public class TelegramAuthorizationManager : ITelegramAuthorizationManager
    {
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;

        public TelegramAuthorizationManager(IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public bool Authorize(long telegramId)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var user = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .FirstOrDefault(x => x.UserAccounts.Any(y =>
                        y is TelegramAccount telegramAccount && telegramAccount.TelegramId == telegramId));

                ActionContext.User = user;
                return user != null;
            }
        }
    }
}