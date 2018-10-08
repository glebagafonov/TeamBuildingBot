using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Base;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;

namespace Bot.Infrastructure.Repositories
{
    public class BotUserRepository : Repository<BotUser>, IBotUserRepository
    {
        public BotUserRepository(ISessionProvider provider, ILogger logger) : base(provider, logger)
        {
        }
    }
}
