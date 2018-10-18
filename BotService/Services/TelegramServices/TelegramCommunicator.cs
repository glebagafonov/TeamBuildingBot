using System.IO;
using System.Threading.Tasks;
using BotService.Services.Interfaces;
using Telegram.Bot.Types;

namespace BotService.Services.TelegramServices
{
    public class TelegramCommunicator : ICommunicator
    {
        public  long                       TelegramId                 { get; }
        private TelegramInteractionService TelegramInteractionService { get; }

        public TelegramCommunicator(long telegramId, TelegramInteractionService telegramInteractionService)
        {
            TelegramInteractionService = telegramInteractionService;
            TelegramId                 = telegramId;
        }

        public void SendMessage(string text)
        {
            TelegramInteractionService.SendMessage(TelegramId, text).Wait();
        }

        public void SendImage(MemoryStream stream)
        {
            TelegramInteractionService.SendImage(TelegramId, stream).Wait();
        }
    }
}