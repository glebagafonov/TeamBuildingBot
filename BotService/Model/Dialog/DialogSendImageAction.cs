using System.IO;
using BotService.Model.Dialog.Interfaces;

namespace BotService.Model.Dialog
{
    class DialogSendImageAction : IDialogSendImageAction
    {
        public int          OrderNumber { get; }
        public MemoryStream ImageStream { get; }

        public DialogSendImageAction(int orderNumber, MemoryStream imageStream)
        {
            OrderNumber = orderNumber;
            ImageStream = imageStream;
        }
    }
}