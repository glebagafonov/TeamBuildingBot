using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog;
using BotService.Model.Dialog.Interfaces;
using BotService.Requests;
using MediatR;

namespace BotService.Services.TelegramServices.TelegramDialogs
{
    public class RegisterTelegramDialog : Dialog<RegisterRequest>
    {
        private readonly IMediator _mediator;

        public RegisterTelegramDialog(ILogger logger, IMediator mediator) : base(logger)
        {
            _mediator = mediator;
            Add("Введи имя");
            Add((message, registerRequestByTelegramAccount) =>
            {
                registerRequestByTelegramAccount.FirstName = message;
                return registerRequestByTelegramAccount;
            });
            Add("Введи фамилию");
            Add((message, registerRequestByTelegramAccount) =>
            {
                registerRequestByTelegramAccount.LastName = message;
                return registerRequestByTelegramAccount;
            });
//            Add("Введи уникальный пароль");
//            Add((message, registerRequestByTelegramAccount) =>
//            {
//                var task = _mediator.Send(new CheckUniquePassword(message));
//                task.Wait();
//                if(@task.Result)
//                    throw new InvalidInputException("Такой пароль уже существует! Придумай свой уникальный пароль.");
//                registerRequestByTelegramAccount.Password = message;
//                return registerRequestByTelegramAccount;
//            });
            Add("Регистрация завершена");
        }
        
        protected override string CommandName => "Регистрация";
        public override void ProcessDialogEnded()
        {
            _mediator.Send(DialogData);

        }
    }
}