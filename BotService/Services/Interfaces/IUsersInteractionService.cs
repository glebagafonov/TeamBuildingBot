using System;
using Bot.Domain.Entities;
using Bot.Domain.Enums;

namespace BotService.Services.Interfaces
{
    public interface IUsersInteractionService
    {
        void StartDialog(EUserAction userAction);
        void ProcessMessage(string message, ICommunicator communicator);
        void SendMessage(string message, BotUser user);
    }
}