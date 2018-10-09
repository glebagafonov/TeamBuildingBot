using System.Threading.Tasks;
using Bot.Domain.Enums;

namespace BotService.Services.TelegramServices.Interfaces
{
    public delegate void CompleteEventHandler();

    public interface ITelegramDialog
    {
        void ProcessMessage(string message, int messageId, long chatId);
        event CompleteEventHandler CompleteEvent;
        ITelegramDialog Create();
    }
}