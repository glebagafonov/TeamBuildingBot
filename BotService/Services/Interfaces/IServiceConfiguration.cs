using System;

namespace BotService.Services.Interfaces
{
    public interface IServiceConfiguration
    {
        string   TelegramToken         { get; }
        TimeSpan StartDayTime          { get; }
        TimeSpan EndDayTime            { get; }
        TimeSpan FirstInviteTime       { get; }
        TimeSpan InviteTime            { get; }
        TimeSpan GameScheduleThreshold { get; }
        int      PlayersPerTeam        { get; }
        TimeSpan StartGameProcess      { get; }
        TimeSpan GameDeadline          { get; }
    }
}