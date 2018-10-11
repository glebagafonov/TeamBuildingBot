using System.Collections.Generic;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog.Interfaces;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices.TelegramDialogs;
using MediatR;

namespace BotService.Services
{
    public class UsersInteractionService : IUsersInteractionService
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private Dictionary<IDialog, List<ICommunicator>> _dialogs;
        public UsersInteractionService(ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        
        public void StartDialog<TDialogData>(EUserAction userAction, TDialogData initialData)
        {
            switch (userAction)
            {
                case EUserAction.Register:
                {
                    _dialogs.Add(new RegisterTelegramDialog(_logger, _mediator), initialData);
                }
                
            }
        }

        public void ProcessMessage(string message, ICommunicator communicator)
        {
            throw new System.NotImplementedException();
        }

        public void SendMessage(string message, BotUser user)
        {
            throw new System.NotImplementedException();
        }
    }
}