using Bot.Domain.Entities;
using Bot.Domain.Enums;

namespace BotService.Services.Interfaces
{
    public interface IUserInteractionService
    {
        void ProcessMessage(BotUser user, ICommunicator communicator, string message);
    }
}