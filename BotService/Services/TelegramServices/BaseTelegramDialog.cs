using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Services.TelegramServices.Interfaces;
using Telegram.Bot;

namespace BotService.Services.TelegramServices
{
    public abstract class BaseTelegramDialog : ITelegramDialog
    {
        private readonly TelegramInteractionService _telegramInteractionService;
        private readonly ILogger _logger;
        private readonly List<DialogAction> _dialogActions;
        private int _dialogState;

        protected BaseTelegramDialog(long chatId,
            TelegramInteractionService telegramInteractionService,
            ILogger logger)
        {
            _telegramInteractionService = telegramInteractionService;
            _logger                     = logger;

            _dialogActions = new List<DialogAction>();
            _dialogState   = 0;

            Init();
            _telegramInteractionService.SendMessage(chatId, CommandName).Wait();
            _telegramInteractionService.SendMessage(chatId, InitMessage).Wait();
        }

        public async void ProcessMessage(string message, int messageId, long chatId)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            if (action == null) ProcessResult();
            try
            {
                action?.Action(message);
            }
            catch (Exception e)
            {
                _logger.Warn(e);
                await _telegramInteractionService.SendMessage(chatId, e.Message, messageId);
                if (action != null)
                    await _telegramInteractionService.SendMessage(chatId, action.Message, messageId);
            }

            _dialogState++;

            await SendNextMessage(messageId, chatId);
        }

        public event CompleteEventHandler CompleteEvent;
        
        protected void RaiseCompleteEvent()
        {
            CompleteEvent?.Invoke();
        }

        private async Task SendNextMessage(int messageId, long chatId)
        {
            var action = _dialogActions.FirstOrDefault(x => x.OrderNumber == _dialogState);
            if (action == null) 
                ProcessResult();
            else
                await _telegramInteractionService.SendMessage(chatId, action.Message);
        }

        protected abstract void Init();
        protected abstract void ProcessResult();
        protected abstract string CommandName { get; }
        protected abstract string InitMessage { get; }
        
        protected virtual void RaiseSampleEvent()
        {
            // Raise the event by using the () operator.
            
        }

        protected void Add(string message, Action<string> action)
        {
            _dialogActions.Add(new DialogAction(_dialogActions.Count, message, action));
        }

        private class DialogAction
        {
            public DialogAction(int orderNumber, string message, Action<string> action)
            {
                OrderNumber = orderNumber;
                Message     = message;
                Action      = action;
            }

            public Action<string> Action      { get; }
            public int            OrderNumber { get; }
            public string         Message     { get; }
        }
    }
}