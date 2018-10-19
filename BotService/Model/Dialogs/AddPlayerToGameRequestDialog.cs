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
using NHibernate.Util;

namespace BotService.Model.Dialogs
{
    public class AddPlayerToGameRequestDialog : Dialog<AddPlayerToGameRequest>
    {
        private readonly IMediator _mediator;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IGameRepository _gameRepository;
        private readonly IServiceConfiguration _serviceConfiguration;
        private bool _hasDesicion;

        public AddPlayerToGameRequestDialog(IEnumerable<ICommunicator> communicators, Guid userId,
            AddPlayerToGameRequest dialogData,
            ILogger logger, IMediator mediator, IThreadContextSessionProvider threadContextSessionProvider,
            IGameRepository gameRepository, IServiceConfiguration serviceConfiguration) : base(logger, communicators,
            userId, dialogData)
        {
            _mediator                     = mediator;
            _threadContextSessionProvider = threadContextSessionProvider;
            _gameRepository               = gameRepository;
            _serviceConfiguration         = serviceConfiguration;
            _hasDesicion                  = false;

            using (threadContextSessionProvider.CreateSessionScope())
            {
                var games = gameRepository.ListBySpecification(new UndeletedEntities<Game>()).ToList();
                games = games.Where(x =>
                        x.AcceptedPlayers.Count + x.RequestedPlayers.Count < serviceConfiguration.PlayersPerTeam * 2 &&
                        x.IsActive &&
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
                if (!games.Any())
                {
                    Add($"Нет игр для добавления игроков");
                }
                else
                {
                    Add($"Введи номер игры, в которую хочешь добавить игроков:");

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

                        data.GameId = games[number - 1].Id;
                        var game = games[number - 1];

                        AddPlayerProcessingMessages(game.Id);

                        return data;
                    });
                }
            }
        }

        private void AddPlayerProcessingMessages(Guid gameId)

        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(gameId);
                var playersForAdd = game.SortedPlayersByRating
                    .Where(x => game.AcceptedPlayers.All(y => y.User.Id != x.Player.User.Id)).ToList();
                var playersMessage = "Игроки:\n" + string.Join("\n",
                                         playersForAdd.Select((x, index) =>
                                             $"{index + 1}. {x.Player.User.FirstName} {x.Player.User.LastName}"));
                Add(playersMessage);
                Add("Введи номера через запятую без пробела, например \"1,3,2\"");
                Add((message, data) =>
                {
                    var stringNumbers = message.Split(',');
                    var numbers       = new List<int>();
                    foreach (var stringNumber in stringNumbers)
                    {
                        if (!int.TryParse(stringNumber, out var number) || (number < 0 && number > 100))
                        {
                            throw new InvalidInputException(
                                $"Не распознал {stringNumber}. Введи номера через запятую без пробела, например \"1,3,2\"");
                        }

                        numbers.Add(number);
                    }

                    var freePlacesCount = _serviceConfiguration.PlayersPerTeam * 2 - game.AcceptedPlayers.Count;
                    if (numbers.Count > freePlacesCount)
                    {
                        throw new InvalidInputException($"Свободно {freePlacesCount} мест. Но ты ввел {numbers.Count}");
                    }

                    data.PlayerIds = playersForAdd.Where((x, y) => numbers.Contains(y + 1)).Select(x => x.Player.Id)
                        .ToList();
                    _hasDesicion = true;
                    return data;
                });
            }
        }


        protected override string CommandName => "Добавление игроков в игру";

        public override void ProcessDialogEnded()
        {
            if (_hasDesicion)
                _mediator.Send(DialogData);
        }
    }
}