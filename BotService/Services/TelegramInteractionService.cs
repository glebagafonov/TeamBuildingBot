using System;
using System.Threading.Tasks;
using BotService.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotService.Services
{
    public class TelegramInteractionService
    {
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly ITelegramBotClient _client;
        
        public TelegramInteractionService(IServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
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