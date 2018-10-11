using System;
using System.Threading.Tasks;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog.Interfaces;
using BotService.Requests;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices.TelegramDialogs;
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
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IMediator _mediator;
        private readonly IBotUserRepository _botUserRepository;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly ILogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly ITelegramBotClient _client;
        private IDialog _currentDialog;

        public TelegramInteractionService(IServiceConfiguration serviceConfiguration,
            IMediator mediator,
            IBotUserRepository botUserRepository,
            IThreadContextSessionProvider threadContextSessionProvider,
            ILogger logger,
            IPlayerRepository playerRepository)
        {
            _serviceConfiguration         = serviceConfiguration;
            _mediator                     = mediator;
            _botUserRepository            = botUserRepository;
            _threadContextSessionProvider = threadContextSessionProvider;
            _logger                       = logger;
            _playerRepository = playerRepository;
            _client                       = new TelegramBotClient(_serviceConfiguration.TelegramToken);
            _client.SetWebhookAsync("");

            _client.OnMessage += Bot_OnMessage;
            //_client.On
            _client.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            try
            {
                if (message.Type == MessageType.Text)
                {
                    if (_currentDialog != null)
                    {
                        if (message.Text.Contains(CancelCommand))
                        {
                            _currentDialog = null;
                            await SendMessage(new ChatId(message.Chat.Id),
                                "Введи команду. (список команд можно получить использовав команду - /help)");
                        }
                        else
                        {
                            
                            //_currentDialog.ProcessMessage(message.Text, message.MessageId, message.Chat.Id);
                        }

                    }
                    else
                    {
                        if (message.Text.Contains("/register"))
                        {
//                            _currentDialog = new RegisterTelegramDialog(message.Chat.Id, message.From.Id, _mediator,
//                                this, _logger).Create();
//                            _currentDialog.CompleteEvent += CompleteEventHandler;
                        }
                        else if (message.Text.Contains("/bindplayer"))
                        {
                            /*_currentDialog = new BindUserToPlayerDialog(message.Chat.Id, message.From.Id, _mediator,
                                this, _logger, _botUserRepository, _threadContextSessionProvider, _playerRepository).Create();
                            _currentDialog.CompleteEvent += CompleteEventHandler;*/
                        }
                        else
                        {
                            await SendMessage(new ChatId(message.Chat.Id),
                                "Не понимаю тебя. Возможно тебе поможет команда - /help");
                        }
                    }
                }
                else
                {
                    await SendMessage(new ChatId(message.Chat.Id), "Я понимаю только текст!", message.MessageId);
                }
            }
            catch (UnauthorizedException exception)
            {
                await SendMessage(new ChatId(message.Chat.Id), "У вас нет прав для выполнения этой команды",
                    message.MessageId);
                _logger.Error(exception);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        private void CompleteEventHandler()
        {
            _currentDialog.CompleteEvent -= CompleteEventHandler;
            _currentDialog = null;
        }

        public async Task SendMessage(ChatId chat, string text, int? messageId = 0)
        {
            await _client.SendTextMessageAsync(chat, text, replyToMessageId: messageId ?? 0);
        }
    }
}