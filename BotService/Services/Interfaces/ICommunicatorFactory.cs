using Bot.Domain.Entities.Base;

namespace BotService.Services.Interfaces
{
    public interface ICommunicatorFactory
    {
        ICommunicator GetCommunicator(BaseAccount account);
    }
}