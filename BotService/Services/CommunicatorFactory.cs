using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Infrastructure.Exceptions;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;

namespace BotService.Services
{
    public class CommunicatorFactory : ICommunicatorFactory
    {
        private readonly TelegramInteractionService _telegramInteractionService;

        public CommunicatorFactory(TelegramInteractionService telegramInteractionService)
        {
            _telegramInteractionService = telegramInteractionService;
        }
        public ICommunicator GetCommunicator(BaseAccount account)
        {
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