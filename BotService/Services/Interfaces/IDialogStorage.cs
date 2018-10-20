using System;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using BotService.Model.Dialog.Interfaces;

namespace BotService.Services.Interfaces
{
    public interface IDialogStorage
    {
        IDialog Get(BotUser user);
        IDialog RemoveDialog(BotUser user);
        IDialog RemoveDialog(Guid userId);
        void SaveDialog(BotUser user, IDialog dialog);
    }
}