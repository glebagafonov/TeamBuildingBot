using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog.Interfaces;
using BotService.Services.Interfaces;

namespace BotService.Model.Dialog
{
    public abstract class Dialog<TDialogData> : IDialog<TDialogData>
        where TDialogData : class, new()
    {
        private readonly ILogger _logger;
        private readonly List<IDialogAction> _dialogActions;

        private Guid _userId;
        private int _dialogState;
        protected TDialogData DialogData;

        protected Dialog(ILogger logger)
        {
            _logger = logger;
            

            _dialogActions = new List<IDialogAction>();
            _dialogState   = 0;
        }

        public IDialog<TDialogData> Start(ICommunicator communicator, Guid userId, TDialogData dialogData)
        {
            _userId = userId;
            DialogData = dialogData;
            communicator.SendMessage(CommandName);
            DoAction(communicator);
            return this;
        }

        private void DoAction(ICommunicator communicator)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            switch (action)
            {
                case IDialogSendMessageAction dialogSendMessageAction:
                {
                    communicator.SendMessage(dialogSendMessageAction.Message);
                    IncreaseState(communicator);
                }
                    break;
                case IDialogSendImageAction dialogSendImageAction:
                {
                    communicator.SendImage(dialogSendImageAction.ImageStream);
                    IncreaseState(communicator);
                }
                    break;
            }
        }

        private void DoAction(string message, ICommunicator communicator)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            switch (action)
            {
                case IDialogSendMessageAction dialogSendMessageAction:
                {
                    communicator.SendMessage(dialogSendMessageAction.Message);
                    IncreaseState(communicator);
                }
                    break;
                case IDialogSendImageAction dialogSendImageAction:
                {
                    communicator.SendImage(dialogSendImageAction.ImageStream);
                    IncreaseState(communicator);
                }
                    break;

                case IDialogUserAction<TDialogData> dialogUserAction:
                {
                    try
                    {
                        DialogData = dialogUserAction?.Action(message, DialogData);
                        IncreaseState(communicator);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn(e);
                        communicator.SendMessage(e.Message);
                        communicator.SendMessage("Повторите попытку или введите команду - /cancel");
                    }
                }
                    break;
            }
        }

        public void IncreaseState(ICommunicator communicator)
        {
            
            _dialogState++;
            
            if (_dialogState == _dialogActions.Count)
            {
                RaiseCompleteEvent();
                ProcessDialogEnded();
            }
            else
            {
                DoAction(communicator);
            }
        }

        public void ProcessMessage(string message, ICommunicator communicator)
        {
            DoAction(message, communicator);
        }

        public event CompleteEventHandler CompleteEvent;

        private void RaiseCompleteEvent()
        {
            CompleteEvent?.Invoke(_userId);
        }

        protected abstract string CommandName { get; }
        public abstract void ProcessDialogEnded();


        public void Add(string message)
        {
            _dialogActions.Add(new DialogSendMessageAction(_dialogActions.Count, message));
        }
        
        public void Add(MemoryStream imageStream)
        {
            _dialogActions.Add(new DialogSendImageAction(_dialogActions.Count, imageStream));
        }

        public void Add(Func<string, TDialogData, TDialogData> action)
        {
            _dialogActions.Add(new DialogUserAction<TDialogData>(_dialogActions.Count, action));
        }
    }
}