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
    public class PlayerGameDeferedDecisionRequestDialog : Dialog<PlayerGameDecisionRequest>
    {
        private readonly IMediator _mediator;
        private bool _hasDecision;

        public PlayerGameDeferedDecisionRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId,
            PlayerGameDecisionRequest dialogData,
            ILogger logger, IMediator mediator, IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository, IServiceConfiguration serviceConfiguration, bool decision) : base(logger,
            communicators,
            userId, dialogData)
        {
            _hasDecision = false;
            _mediator    = mediator;
            using (threadContextSessionProvider.CreateSessionScope())
            {
                var games = gameRepository.ListBySpecification(new UndeletedEntities<Game>()).ToList();
                games = games.Where(x => x.IsActive &&
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
                    .ToList();

                games = decision
                    ? games.Where(x => x.RequestedPlayers.Any(y => y.Player.User.Id == userId)).Take(20).ToList()
                    : games.Where(x => x.AcceptedPlayers.Any(y => y.User.Id == userId)).Take(20).ToList();


                if (games.Count == 0)
                {
                    Add($"Нет игр, по которым ты можешь выбрать решение");
                }
                else
                {
                    var decisionMessage = decision ? "которую хочешь принять" : "от которой хочешь отказаться";
                    Add($"Введи номер игры, {decisionMessage}:");

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

                        _hasDecision  = true;
                        data.GameId   = games[number - 1].Id;
                        data.Decision = decision;
                        data.PlayerId = games[number - 1].AcceptedPlayers.First(x => x.User.Id == userId).Id;

                        return data;
                    });
                }
            }
        }


        protected override string CommandName => "Решение по игре";

        public override void ProcessDialogEnded()
        {
            if (_hasDecision)
                _mediator.Send(DialogData);
        }
    }
}