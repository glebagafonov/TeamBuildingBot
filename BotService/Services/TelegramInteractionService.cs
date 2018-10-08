using System;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Requests;
using BotService.Services.Interfaces;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotService.Services
{
    public class TelegramInteractionService
    {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IMediator _mediator;
        private readonly IBotUserRepository _botUserRepository;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly ITelegramBotClient _client;
        
        public TelegramInteractionService(IServiceConfiguration serviceConfiguration,
            IMediator mediator,
            IBotUserRepository botUserRepository,
            IThreadContextSessionProvider threadContextSessionProvider)
        {
            _serviceConfiguration = serviceConfiguration;
            _mediator = mediator;
            _botUserRepository = botUserRepository;
            _threadContextSessionProvider = threadContextSessionProvider;
            _client = new TelegramBotClient(_serviceConfiguration.TelegramToken);
            _client.SetWebhookAsync("");
            
            _client.OnMessage += Bot_OnMessage;
            //_client.On
            _client.StartReceiving();
        }
        
        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Type == MessageType.Text)
            {
                if (message.Text.Contains("/register"))
                {
                    await _mediator.Send(new RegisterRequest());
                    await SendMessage(new ChatId(message.Chat.Id), message.Text, message.MessageId);
                }
                else
                {
                    
                    Console.WriteLine($@"Received a text message in chat {e.Message.Chat.Id}.");

                    await SendMessage(new ChatId(message.Chat.Id), message.Text);
                }
            }
        }

        private async Task SendMessage(ChatId chat, string text, int? messageId = 0)
        {
            await _client.SendTextMessageAsync(chat, "You said:\n" + text, replyToMessageId: messageId ?? 0);
        }
    }
}