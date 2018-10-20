using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Services;

namespace BotService.Providers
{
    public class BotServiceSessionProvider : ThreadContextSessionProvider
    {
        public BotServiceSessionProvider(ILogger logger, BotServiceSessionFactory sessionFactory) : base(logger, sessionFactory)
        {
        }
    }
}