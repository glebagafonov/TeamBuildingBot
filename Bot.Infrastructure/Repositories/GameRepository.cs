using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Base;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;

namespace Bot.Infrastructure.Repositories
{
    public class GameRepository : Repository<Game>, IGameRepository
    {
        public GameRepository(ISessionProvider provider, ILogger logger) : base(provider, logger)
        {
        }
    }
}
