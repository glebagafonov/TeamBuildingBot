using BotService.Services.Interfaces;
using Telegram.Bot.Types;

namespace BotService.Services.TelegramServices
{
    public class TelegramCommunicator : ICommunicator
    {
        public long                       ChatId                     { get; }
        public long                       TelegramId                 { get; }
        public TelegramInteractionService TelegramInteractionService { get; }

        public TelegramCommunicator(long chatId, long telegramId, TelegramInteractionService telegramInteractionService)
        {
            ChatId                     = chatId;
            TelegramInteractionService = telegramInteractionService;
            TelegramId                 = telegramId;
        }

        public async void SendMessage(string text)
        {
            await TelegramInteractionService.SendMessage(ChatId, text);
        }
    }
}