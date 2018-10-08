using System;
using System.Collections.Generic;
using Bot.Domain.Entities;

namespace BotService.Services
{
    public interface IBotCommandsProcessor
    {
        //administrator commands
        Guid AddUser(string firstName, string lastName);
        void BindUser(Guid userId, int telegramId);
        
        void SetPlayersCountPerTeam(int n);
        
        void CancelScheduledGame(Guid gameId);
        void ScheduleGame(DateTime dateTime);
        ICollection<Game> GetScheduledGames();
        
        void SetPlayerParticipationRatio(Guid userId, int participationRatio);
        void SetPlayerSkillValue(Guid userId, int skillValue);
        
        //user commands
        void AcceptGame(Guid userId);
        void DeclineGame(Guid userId);
        void SetPlayerInactive(Guid userId);
        void Register(string firstName, string lastName);

    }
}