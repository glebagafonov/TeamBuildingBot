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
            dialogData.CurrentUserId = userId;
            _mediator = mediator;
            
            Add("Введи логин");
            Add((message, data) =>
            {
                data.Login = message;
                return data;
            });
            Add("Введи уникальный пароль");
            Add((message, data) =>
            {
                data.Password = message;
                var result = _mediator.Send(data).Result;
                Add(!result
                    ? "Либо логин неверный, либо пароль... :(. Попробуй еще раз через - /auth"
                    : "Авторизация прошла успешно");
                return data;
            });
            
        }

        protected override string CommandName => "Авторизация";

        public override void ProcessDialogEnded()
        {
        }
    }
}