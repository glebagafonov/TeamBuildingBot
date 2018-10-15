using System;
using System.IO;
using Bot.Domain.Entities;
using BotService.Services.Interfaces;

namespace BotService.Model.Dialog.Interfaces
{
    public delegate void CompleteEventHandler(Guid userId);

    public interface IDialog<TDialogData> : IDialog
        where TDialogData : class
    {
        IDialog<TDialogData> Start(ICommunicator communicator, Guid userId, TDialogData dialogData);

        void Add(string message);
        void Add(MemoryStream imageStream);
        void Add(Func<string, TDialogData, TDialogData> action);
    }

    public interface IDialog
    {
        void ProcessMessage(string message, ICommunicator communicator);
        event CompleteEventHandler CompleteEvent;
        void ProcessDialogEnded();
    }
}