namespace BotService.Services.Interfaces
{
    public interface ITelegramAuthorizationManager : IAuthorizationManager
    {
        bool Authorize(long telegramId);
    }
}