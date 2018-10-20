using System;
using System.Collections.Generic;
using Bot.Domain.Entities;
using BotService.Model.Dialog.Interfaces;
using BotService.Services.Interfaces;

namespace BotService.Services
{
    public class DialogStorage : IDialogStorage
    {
        private Dictionary<Guid, IDialog> _dialogs;

        public DialogStorage()
        {
            _dialogs = new Dictionary<Guid, IDialog>();
        }

        public IDialog Get(BotUser user)
        {
            return !_dialogs.ContainsKey(user.Id) ? null : _dialogs[user.Id];
        }

        public IDialog RemoveDialog(BotUser user)
        {
            return RemoveDialog(user.Id);
        }

        public IDialog RemoveDialog(Guid userId)
        {
            if (_dialogs.ContainsKey(userId))
            {
                var dialog = _dialogs[userId];
               _dialogs.Remove(userId);
                return dialog;
            }

            return null;
        }

        public void SaveDialog(BotUser user, IDialog dialog)
        {
            if (_dialogs.ContainsKey(user.Id))
                throw new InvalidOperationException("User already has dialog");
            _dialogs.Add(user.Id, dialog);
        }
    }
}