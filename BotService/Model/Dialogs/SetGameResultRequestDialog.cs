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
    public class SetGameResultRequestDialog : Dialog<SetGameResultRequest>
    {
        private readonly IMediator                     _mediator;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IGameRepository               _gameRepository;

        public SetGameResultRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId,
            SetGameResultRequest dialogData,
            ILogger logger, IMediator mediator, IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository) : base(logger, communicators,
            userId, dialogData)
        {
            _mediator                     = mediator;
            _threadContextSessionProvider = threadContextSessionProvider;
            _gameRepository               = gameRepository;
            using (threadContextSessionProvider.CreateSessionScope())
            {
                var games = gameRepository.ListBySpecification(new UndeletedEntities<Game>())
                    .Where(x => DateTime.Now > x.DateTime && !x.ResultSet).ToList();
                
                if (!games.Any())
                {
                    Add($"Нет игр для внесения результата");
                }
                else
                {
                    var startMessage = $"Игры без результатов:\n";
                    startMessage += string.Join("\n", games.Select((x, index) => $"{index + 1}. {x.DateTime}"));
                    Add(startMessage);
                    Add("Введи номер игры, по которой хочешь внести результат");
                    Add((message, data) =>
                    {
                        if (!int.TryParse(message, out var number) || (number < 1 && number > games.Count()))
                        {
                            throw new InvalidInputException($"Введи номер игры от 1 до {games.Count()}");
                        }

                        data.GameId = games[number - 1].Id;

                        return data;
                    });


                    Add("Введи нразницу голов:");
                    var goalDifference = 0;
                    Add((message, data) =>
                    {
                        if (!int.TryParse(message, out var number) || number < 0)
                        {
                            throw new InvalidInputException($"Введи положительное число");
                        }

                        data.GoalDifference = number;
                        goalDifference      = number;
                        if (goalDifference == 0)
                        {
                            data.TeamWinningNumber = 1;
                        }
                        else
                        {
                            SetWinnerDialog(data.GameId);
                        }

                        return data;
                    });
                }
            }
        }

        private void SetWinnerDialog(Guid gameId)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game         = _gameRepository.Get(gameId);
                var teamsMessage = "Команда 1:\n";
                teamsMessage += string.Join("\n",
                    game.DistributedPlayers.Where(x => x.TeamNumber == 1).Select((x, index) =>
                        $"{x.Player.User.FirstName} {x.Player.User.LastName}"));
                teamsMessage += "\n\nКоманда 2:\n";
                teamsMessage += string.Join("\n",
                    game.DistributedPlayers.Where(x => x.TeamNumber == 2).Select((x, index) =>
                        $"{x.Player.User.FirstName} {x.Player.User.LastName}"));

                Add(teamsMessage);
                Add("Введи номер победившей команды:");
                Add((message1, data1) =>
                {
                    if (!int.TryParse(message1, out var number1) || (number1 != 1 && number1 != 2))
                    {
                        throw new InvalidInputException($"Введи номер команды 1 или 2");
                    }

                    data1.TeamWinningNumber = number1;

                    return data1;
                });
                Add("Результат внесен");
            }
        }


        protected override string CommandName => "Внесение результата по игре";

        public override void ProcessDialogEnded()
        {
            _mediator.Send(DialogData);
        }
    }
}