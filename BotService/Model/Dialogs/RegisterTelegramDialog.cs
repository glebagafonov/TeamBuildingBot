using System;
using System.Collections.Generic;
using System.IO;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog;
using BotService.Services.Interfaces;
using CaptchaGen;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class RegisterTelegramDialog : Dialog<RegisterRequest>
    {
        private string _captcha;

        private readonly IMediator _mediator;

        public RegisterTelegramDialog(IEnumerable<ICommunicator> communicators, Guid userId, RegisterRequest dialogData,
            ILogger logger, IMediator mediator) : base(logger, communicators, userId, dialogData)
        {
            _mediator = mediator;
            
            
            _captcha   = CaptchaCodeFactory.GenerateCaptchaCode(4);
            CaptchaAction();
        }

        private void CaptchaAction()
        {
//            Add("Введи капчу или используй команду - /cancel");
//            var image = ImageFactory.GenerateImage(_captcha);
//            Add(image);
//            Add((message, registerRequestByTelegramAccount) =>
//            {
//                if (message != _captcha)
//                {
//                    _captcha = CaptchaCodeFactory.GenerateCaptchaCode(4);
//                    
//                    CaptchaAction();
//                }
//                else
               // {
                    RegisterBranch();
               // }

                //return registerRequestByTelegramAccount;
           // });
        }

        private void RegisterBranch()
        {
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