using System;

namespace BotService.Model.Dialog.Interfaces
{
    public interface IDialogUserAction<TDialogData> : IDialogAction
    where TDialogData: class
    {
        Func<string, TDialogData, TDialogData> Action { get; set; }
    }
}