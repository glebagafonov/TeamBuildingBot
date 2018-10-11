namespace BotService.Model.Dialog.Interfaces
{
    public interface IDialogSendMessageAction : IDialogAction
    {
        string Message { get; }
    }
}