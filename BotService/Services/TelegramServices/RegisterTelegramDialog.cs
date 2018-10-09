using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Requests;
using MediatR;

namespace BotService.Services.TelegramServices
{
    public class RegisterTelegramDialog : BaseTelegramDialog
    {
        private readonly long _chatId;
        private readonly long _telegramId;
        private readonly IMediator _mediator;
        private readonly TelegramInteractionService _telegramInteractionService;
        private RegisterRequestByTelegramAccount _request;

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

        protected override async void Init()
        {
            Add("Введите имя:", x =>
            {
                if (string.IsNullOrEmpty(x))
                {
                    throw new InvalidInputException("Введена пустая строка. Попробуй еще раз");
                }

                _request.FirstName = x;
            });
            Add("Введите фамилию:", x =>
                {
                    if (string.IsNullOrEmpty(x))
                    {
                        throw new InvalidInputException("Введена пустая строка. Попробуй еще раз");
                    }

                    _request.LastName = x;
                }
            );
        }

        protected override async void ProcessResult()
        {
            _request.TelegramId = _telegramId;
            await _mediator.Send(_request);
            await _telegramInteractionService.SendMessage(_chatId, "Успешная регистрация");

            RaiseCompleteEvent();
        }

        protected override string CommandName => "Регистрация";
        protected override string InitMessage => "Введите имя:";
    }
}