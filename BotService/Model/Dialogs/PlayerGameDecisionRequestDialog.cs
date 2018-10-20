using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class PlayerGameDecisionRequestDialog : Dialog<PlayerGameDecisionRequest>
    {
        private readonly IMediator _mediator;

        public PlayerGameDecisionRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId, PlayerGameDecisionRequest dialogData,
            ILogger logger, IMediator mediator, IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository) : base(logger, communicators,
            userId, dialogData)
        {
            _mediator = mediator;
            using (threadContextSessionProvider.CreateSessionScope())
            {
                var game = gameRepository.Get(dialogData.GameId);

                Add($"Играешь {game.DateTime}? (Ответь - \"да\" или \"нет\" или отмени диалог командой - /cancel. Если отменишь диалог, то принять решение можно немного позже, используя команды - /accept, /reject)");


                Add((message, data) =>
                {
                    if (!message.CaseInsensitiveContains("да") && !message.CaseInsensitiveContains("нет"))
                    {
                        throw new InvalidInputException("Введи \"да\" или \"нет\"");
                    }

                    if (message.CaseInsensitiveContains("да"))
                    {
                        data.Decision = true;
                    }

                    if (message.CaseInsensitiveContains("нет"))

                    {
                        data.Decision = false;
                    }

                    return data;
                });
                Add($"Твой ответ учтен");
            }
        }

        

        protected override string CommandName => "Решение по игре";
        public override void ProcessDialogEnded()
        {
            _mediator.Send(DialogData);

        }
    }
}