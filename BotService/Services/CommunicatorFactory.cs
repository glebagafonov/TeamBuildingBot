using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;

namespace BotService.Services
{
    public class CommunicatorFactory : ICommunicatorFactory
    {
        private TelegramInteractionService _telegramInteractionService;

        public ICommunicator GetCommunicator(BaseAccount account)
        {
            _telegramInteractionService = ServiceLocator.Get<TelegramInteractionService>();
            switch (account)
            {
                case TelegramAccount telegramAccount:
                {
                    return _telegramInteractionService.GetCommunicator(telegramAccount);
                }
                default:
                    throw new InvalidInputException();
            }
        }
    }
}