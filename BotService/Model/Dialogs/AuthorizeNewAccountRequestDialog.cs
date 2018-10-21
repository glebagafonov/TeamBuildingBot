using System;
using System.Collections.Generic;
using System.IO;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog;
using BotService.Services.Interfaces;
using CaptchaGen;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class AuthorizeNewAccountRequestDialog : Dialog<AuthorizeNewAccountRequest>
    {
        private readonly IMediator _mediator;

        public AuthorizeNewAccountRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId, AuthorizeNewAccountRequest dialogData,
            ILogger logger, IMediator mediator) : base(logger, communicators, userId, dialogData)
        {
            
            _mediator = mediator;
            
            Add("Введи свой уникальный пароль");
            Add((message, registerRequestByTelegramAccount) =>
            {
                var result = _mediator.Send(new AuthorizeNewAccountRequest()
                    {CurrentUserId = userId, Password = message}).Result;
                if (!result)
                {
                    throw new InvalidInputException("Аккаунта с таким паролем не существует");
                }

                return registerRequestByTelegramAccount;
            });
            Add("Авторизация прошла успешно");
        }

        protected override string CommandName => "Авторизация";

        public override void ProcessDialogEnded()
        {
        }
    }
}