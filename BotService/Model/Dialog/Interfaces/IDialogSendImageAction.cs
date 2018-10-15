using System.IO;

namespace BotService.Model.Dialog.Interfaces
{
    public interface IDialogSendImageAction : IDialogAction
    {
        MemoryStream ImageStream { get; }
    }
}