using BotService.Model.Dialog.Interfaces;

namespace BotService.Model.Dialog
{
    class DialogSendMessageAction : IDialogSendMessageAction
    {
        public int    OrderNumber { get; }
        public string Message     { get; }

        public DialogSendMessageAction(int orderNumber, string message)
        {
            OrderNumber = orderNumber;
            Message     = message;
        }
    }
}