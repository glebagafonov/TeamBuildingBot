using System;
using System.IO;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog.Interfaces;
using BotService.Model.Dialogs;
using BotService.Services.Interfaces;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotService.Services.TelegramServices
{
    public class TelegramInteractionService
    {
        private const string CancelCommand = "/cancel";
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ITelegramBotClient _client;

        public TelegramInteractionService(IServiceConfiguration serviceConfiguration,
            ILogger logger,
            IMediator mediator,
            IUserInteractionService userInteractionService)
        {
            _logger                 = logger;
            _mediator               = mediator;
            _userInteractionService = userInteractionService;
            _client                 = new TelegramBotClient(serviceConfiguration.TelegramToken);
            _client.SetWebhookAsync("");

            _client.OnMessage += Bot_OnMessage;
            _client.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;


            _logger.Info($"[{message.From.Id}]: Got message");
            try
            {
                var communicator = GetCommunicator(message);
                var user         = await _mediator.Send(new AuthorizeRequest(communicator));

                if (message.Type == MessageType.Text)
                {
                    _userInteractionService.ProcessMessage(user, GetCommunicator(message), message.Text);
                }
                else
                {
                    await SendMessage(message.Chat.Id, "Я понимаю только текст!", message.MessageId);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        public TelegramCommunicator GetCommunicator(TelegramAccount account)
        {
            return new TelegramCommunicator(account.TelegramId, this);
        }

        private TelegramCommunicator GetCommunicator(Message message)
        {
            return new TelegramCommunicator(message.From.Id, this);
        }

        public async Task SendMessage(long telegramId, string text, int? messageId = 0)
        {
            _logger.Trace($"[{telegramId}]: Send message");
            await _client.SendTextMessageAsync(telegramId, text, replyToMessageId: messageId ?? 0);
        }
        
        public async Task SendImage(ChatId chat, MemoryStream stream, int? messageId = 0)
        {
            stream.Position = 0;
            await _client.SendPhotoAsync(chat, stream);
        }
    }
}