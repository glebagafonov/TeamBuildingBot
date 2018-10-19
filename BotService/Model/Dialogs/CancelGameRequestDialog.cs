using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Model.Dialog;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Model.Dialogs
{
    public class CancelGameRequestDialog : Dialog<CancelGameRequest>
    {
        private readonly IMediator _mediator;
        private bool _hasDecision;

        public CancelGameRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId,
            CancelGameRequest dialogData,
            ILogger logger, IMediator mediator, IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository, IServiceConfiguration serviceConfiguration) : base(logger, communicators,
            userId, dialogData)
        {
            _mediator    = mediator;
            _hasDecision = false;

            using (threadContextSessionProvider.CreateSessionScope())
            {
                var games = gameRepository.ListBySpecification(new UndeletedEntities<Game>()).Where(x => x.IsActive &&
                                                                                                         x.DateTime >
                                                                                                         DateTime.Now
                                                                                                             .Add(
                                                                                                                 serviceConfiguration
                                                                                                                     .GameDeadline,
                                                                                                                 serviceConfiguration
                                                                                                                     .StartDayTime,
                                                                                                                 serviceConfiguration
                                                                                                                     .EndDayTime))
                    .OrderBy(x => x.DateTime)
                    .Take(20).ToList();
                if (games.Count == 0)
                {
                    Add($"Нет игр для отмены");
                }
                else
                {
                    Add($"Введи номер игры, которую хочешь отменить:");

                    var gamesMessage = string.Join("\n",
                        games.Select((x, index) =>
                            $"{index + 1}. Дата: {x.DateTime.ToString(DateTimeHelper.DateFormat)}"));
                    Add(gamesMessage);

                    Add((message, data) =>
                    {
                        if (!int.TryParse(message, out var number) || (number < 0 && number > 100))
                        {
                            throw new InvalidInputException($"Введи номер игры от 1 до {games.Count()}");
                        }

                        _hasDecision = true;
                        data.GameId  = games[number - 1].Id;

                        return data;
                    });
                }
            }
        }


        protected override string CommandName => "Отмена игры";

        public override void ProcessDialogEnded()
        {
            if (_hasDecision)
                _mediator.Send(DialogData);
        }
    }
}