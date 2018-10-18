using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog.Interfaces;
using BotService.Services.Interfaces;
using NHibernate.Util;

namespace BotService.Model.Dialog
{
    public abstract class Dialog<TDialogData> : IDialog<TDialogData>
        where TDialogData : class, new()
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ICommunicator> _communicators;
        private readonly List<IDialogAction> _dialogActions;

        private Guid _userId;
        private int _dialogState;
        protected TDialogData DialogData;

        protected Dialog(ILogger logger, IEnumerable<ICommunicator> communicators, Guid userId, TDialogData dialogData)
        {
            _logger = logger;
            _communicators = communicators;
            _dialogActions = new List<IDialogAction>();
            _dialogState   = 0;
            
            _userId    = userId;
            DialogData = dialogData;
        }

        public IDialog Start()
        {
            foreach (var x in _communicators) x.SendMessage(CommandName);
            DoAction(_communicators);
            return this;
        }

        private void DoAction(IEnumerable<ICommunicator> communicator)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            switch (action)
            {
                case IDialogSendMessageAction dialogSendMessageAction:
                {
                    foreach (var x in _communicators) x.SendMessage(dialogSendMessageAction.Message);
                    IncreaseState(communicator);
                }
                    break;
                case IDialogSendImageAction dialogSendImageAction:
                {
                    foreach (var x in _communicators) x.SendImage(dialogSendImageAction.ImageStream);
                    IncreaseState(communicator);
                }
                    break;
            }
        }

        private void DoAction(string message, IEnumerable<ICommunicator> communicators)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            switch (action)
            {
                case IDialogSendMessageAction dialogSendMessageAction:
                {
                    foreach (var x in _communicators) x.SendMessage(dialogSendMessageAction.Message);
                    IncreaseState(communicators);
                }
                    break;
                case IDialogSendImageAction dialogSendImageAction:
                {
                    foreach (var x in _communicators) x.SendImage(dialogSendImageAction.ImageStream);
                    IncreaseState(communicators);
                }
                    break;

                case IDialogUserAction<TDialogData> dialogUserAction:
                {
                    try
                    {
                        DialogData = dialogUserAction?.Action(message, DialogData);
                        IncreaseState(communicators);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn(e);
                        foreach (var x in _communicators) x.SendMessage(e.Message);
                        foreach (var x in _communicators) x.SendMessage("Повторите попытку или введите команду - /cancel");
                    }
                }
                    break;
            }
        }

        public void IncreaseState(IEnumerable<ICommunicator> communicators)
        {
            
            _dialogState++;
            
            if (_dialogState == _dialogActions.Count)
            {
                RaiseCompleteEvent();
                ProcessDialogEnded();
            }
            else
            {
                DoAction(communicators);
            }
        }

        public void ProcessMessage(string message, ICommunicator communicator)
        {
            DoAction(message, new List<ICommunicator>() {communicator});
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