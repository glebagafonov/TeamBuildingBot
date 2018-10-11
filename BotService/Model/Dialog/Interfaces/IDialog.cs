using System;
using BotService.Services.Interfaces;

namespace BotService.Model.Dialog.Interfaces
{
    public delegate void CompleteEventHandler();

    public interface IDialog<TDialogData> : IDialog
        where TDialogData : class
    {
        IDialog<TDialogData> Start(ICommunicator communicator, TDialogData dialogData);

        void Add(string message);
        void Add(Func<string, TDialogData, TDialogData> action);
    }

    public interface IDialog
    {
        void ProcessMessage(string message, ICommunicator communicator);
        event CompleteEventHandler CompleteEvent;
        void ProcessDialogEnded();
    }
}