using System;
using System.Collections.Generic;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;

namespace BotService.Services.Interfaces
{
    public interface IUserInteractionService
    {
        void ProcessMessage(BotUser user, ICommunicator communicator, string message);
        void SendMessage(string text, BotUser user);
        
        //start dialogs 
        void StartGameConfirmationDialog(Player player, List<ICommunicator> communicator, Guid gameId);
    }
}