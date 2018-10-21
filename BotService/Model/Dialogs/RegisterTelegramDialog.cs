using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Bot.Infrastructure.Exceptions;
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
        private static readonly Regex LoginValidator = new Regex(@"^\w+$");
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
            Add("Введи логин. Логин может состоять из англйских букв любого регистра, цифр, символа подчеркивания. Минимальная длина - 5 символов");
            Add((message, registerRequestByTelegramAccount) =>
            {
                if (message.Length < 5 || !LoginValidator.IsMatch(message))
                {
                    throw new InvalidInputException("Логин уне удовлетворяет правилам");
                }

                if (_mediator.Send(new CheckUniqueLogin(message)).Result)
                {
                    throw new InvalidInputException("Такой логин уже существует! Придумай свой уникальный логин.");
                }
                registerRequestByTelegramAccount.Login = message;
                return registerRequestByTelegramAccount;
            });
            Add("Введи пароль. Не менее 6 символов");
            Add((message, registerRequestByTelegramAccount) =>
            {
                if (message.Length < 6)
                {
                    throw new InvalidInputException("Пароль уне удовлетворяет правилам");
                }
               
                registerRequestByTelegramAccount.Password = message;
                return registerRequestByTelegramAccount;
            });
            Add("Регистрация завершена");
        }

        protected override string CommandName => "Регистрация";

        public override void ProcessDialogEnded()
        {
                _mediator.Send(DialogData);
        }
    }
}