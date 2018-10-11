using System;
using BotService.Model.Dialog.Interfaces;

namespace BotService.Model.Dialog
{
    class DialogUserAction<TDialogData> : IDialogUserAction<TDialogData> where TDialogData : class
    {
        public int                                    OrderNumber { get; }
        public Func<string, TDialogData, TDialogData> Action      { get; set; }

        public DialogUserAction(int orderNumber, Func<string, TDialogData, TDialogData> action)
        {
            OrderNumber = orderNumber;
            Action      = action;
        }
    }
}