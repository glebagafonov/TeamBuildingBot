using System.IO;
using System.Threading.Tasks;
using BotService.Services.Interfaces;
using Telegram.Bot.Types;

namespace BotService.Services.TelegramServices
{
    public class TelegramCommunicator : ICommunicator
    {
        private long                       ChatId                     { get; }
        public  long                       TelegramId                 { get; }
        private TelegramInteractionService TelegramInteractionService { get; }

        public TelegramCommunicator(long chatId, long telegramId, TelegramInteractionService telegramInteractionService)
        {
            ChatId                     = chatId;
            TelegramInteractionService = telegramInteractionService;
            TelegramId                 = telegramId;
        }

        public void SendMessage(string text)
        {
            TelegramInteractionService.SendMessage(ChatId, text).Wait();
        }

        public void SendImage(MemoryStream stream)
        {
            TelegramInteractionService.SendImage(ChatId, stream).Wait();
        }
    }
}