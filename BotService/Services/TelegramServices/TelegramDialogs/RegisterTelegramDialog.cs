using Bot.Infrastructure.Services.Interfaces;
using BotService.Requests;
using BotService.Services.TelegramServices.TelegramDialogs.Base;
using MediatR;

namespace BotService.Services.TelegramServices.TelegramDialogs
{
    public class RegisterTelegramDialog : BaseTelegramDialog
    {
        private readonly long _chatId;
        private readonly IMediator _mediator;
        private readonly long _telegramId;
        private readonly TelegramInteractionService _telegramInteractionService;
        private readonly RegisterRequestByTelegramAccount _request;

        public RegisterTelegramDialog(long chatId, long telegramId, IMediator mediator,
            TelegramInteractionService telegramInteractionService, ILogger logger) : base(chatId,
            telegramInteractionService, logger)
        {
            _chatId                     = chatId;
            _telegramId                 = telegramId;
            _mediator                   = mediator;
            _telegramInteractionService = telegramInteractionService;
            _request                    = new RegisterRequestByTelegramAccount();
        }

        protected override string CommandName => "Регистрация";

        protected override async void Init()
        {
            Add("Введите имя:", x => { _request.FirstName    = x; });
            Add("Введите фамилию:", x => { _request.LastName = x; });
        }

        protected override async void ProcessResult()
        {
            _request.TelegramId = _telegramId;
            await _mediator.Send(_request);
            await _telegramInteractionService.SendMessage(_chatId, "Успешная регистрация");

            RaiseCompleteEvent();
        }

        protected override async void InitAction()
        {
            await _telegramInteractionService.SendMessage(_chatId, "Введите имя");
        }
    }
}