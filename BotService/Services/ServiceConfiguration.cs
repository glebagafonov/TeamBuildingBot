using System.Configuration;
using BotService.Services.Interfaces;

namespace BotService.Services
{
    public class ServiceConfiguration : IServiceConfiguration
    {
        public string TelegramToken => ConfigurationManager.AppSettings["telegramToken"];
    }
}