using System;
using System.Configuration;
using System.Globalization;
using Bot.Infrastructure.Helpers;
using BotService.Services.Interfaces;

namespace BotService.Services
{
    public class ServiceConfiguration : IServiceConfiguration
    {
        public string TelegramToken => ConfigurationManager.AppSettings["telegramToken"];

        public string VkToken => ConfigurationManager.AppSettings["vkToken"];

        public TimeSpan StartDayTime => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["startDayTime"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["startDayTime"].Split(':')[1]),  0);

        public TimeSpan EndDayTime => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["endDayTime"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["endDayTime"].Split(':')[1]),  0);

        public TimeSpan FirstInviteTime => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["firstInviteTime"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["firstInviteTime"].Split(':')[1]),  0);

        public TimeSpan InviteTime => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["inviteTime"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["inviteTime"].Split(':')[1]),  0);

        public TimeSpan GameScheduleThreshold => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["gameScheduleThreshold"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["gameScheduleThreshold"].Split(':')[1]), 0);

        public int PlayersPerTeam => int.Parse(ConfigurationManager.AppSettings["playersPerTeam"]);
        
        public TimeSpan StartGameProcess => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["startGameProcess"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["startGameProcess"].Split(':')[1]), 0);
        
        public TimeSpan GameDeadline => new TimeSpan(int.Parse(ConfigurationManager.AppSettings["gameDeadline"].Split(':')[0]),
            int.Parse(ConfigurationManager.AppSettings["gameDeadline"].Split(':')[1]), 0);
    }
}