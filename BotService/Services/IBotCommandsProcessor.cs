using System;
using System.Collections.Generic;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;

namespace BotService.Services
{
    public interface IBotCommandsProcessor
    {
        //administrator commands
        [UserAccess(EUserRole.Administrator)]
        Guid AddUser(string firstName, string lastName, int telegramId);
        [UserAccess(EUserRole.Administrator)]
        void SetRole(Guid userId, int telegramId);
        
        [UserAccess(EUserRole.Administrator)]
        void SetPlayersCountPerTeam(int n);
        
        [UserAccess(EUserRole.Moderator)]
        void CancelScheduledGame(Guid gameId);
        [UserAccess(EUserRole.Moderator)]
        void ScheduleGame(DateTime dateTime);
        [UserAccess(EUserRole.Moderator)]
        ICollection<Game> GetScheduledGames();
        
        [UserAccess(EUserRole.Moderator)]
        void SetPlayerParticipationRatio(Guid userId, int participationRatio);
        [UserAccess(EUserRole.Moderator)]
        void SetPlayerSkillValue(Guid userId, int skillValue);
        
        //user commands
        [UserAccess(EUserRole.Player)]
        void AcceptGame(Guid userId);
        [UserAccess(EUserRole.Player)]
        void DeclineGame(Guid userId);
        [UserAccess(EUserRole.Player)]
        void SetPlayerInactive(Guid userId);
        [UserAccess(EUserRole.Player)]
        void Register(string firstName, string lastName);

    }
}