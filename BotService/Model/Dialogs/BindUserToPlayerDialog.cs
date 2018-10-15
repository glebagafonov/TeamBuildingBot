using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Model.Dialog;
using BotService.Requests;
using MediatR;
using NHibernate.Hql.Ast;

namespace BotService.Model.Dialogs
{
    public class BindUserToPlayerDialog : Dialog<BindUserToPlayerRequest>
    {
        private readonly IMediator _mediator;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;
        private readonly IPlayerRepository _playerRepository;

        private List<(int number, Guid userId, string Username)> _notBindedUsers;

        public BindUserToPlayerDialog(ILogger logger, IMediator mediator,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository,
            IPlayerRepository playerRepository) : base(logger)
        {
            _mediator                     = mediator;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _playerRepository             = playerRepository;

            _notBindedUsers = GetNotBindedUsers();
            if (!_notBindedUsers.Any())
            {
                Add("Все зарегистрированные пользователи являются игроками");
            }
            else
            {
                var list = "Список пользователей:\n";
                Add(list + string.Join("\n", _notBindedUsers.Select(x => $"{x.number}. {x.Username}")));
                Add("Введи номер пользователя");
                Add((message, registerRequestByTelegramAccount) =>
                {
                    if (!int.TryParse(message, out var number))
                    {
                        throw new InvalidInputException("Введен не номер");
                    }

                    if (number < 1 && number > _notBindedUsers.Count)
                        throw new InvalidInputException("Номер вне диапазона");

                    registerRequestByTelegramAccount.UserId =
                        _notBindedUsers.FirstOrDefault(x => x.number == number).userId;
                    return registerRequestByTelegramAccount;
                });
                Add("Введи коэффициент умения");
                Add((message, bindUserToPlayerRequest) =>
                {
                    if (!int.TryParse(message, out var number) || (number < 0 && number > 100))
                    {
                        throw new InvalidInputException("Введите число от 0 до 100");
                    }

                    bindUserToPlayerRequest.SkillValue = number;
                    return bindUserToPlayerRequest;
                });
                Add("Введи коэффициент посещаемости");
                Add((message, bindUserToPlayerRequest) =>
                {
                    if (!int.TryParse(message, out var number) || (number < 0 && number > 100))
                    {
                        throw new InvalidInputException("Введите число от 0 до 100");
                    }

                    bindUserToPlayerRequest.ParticipationRatio = number;
                    return bindUserToPlayerRequest;
                });
                Add("Введите активен ли игрок? (да/нет)");
                Add((message, bindUserToPlayerRequest) =>
                {
                    if (!message.CaseInsensitiveContains("да") && !message.CaseInsensitiveContains("нет"))
                    {
                        throw new InvalidInputException("Введите \"да\" или \"нет\"");
                    }

                    if (message.CaseInsensitiveContains("да"))
                    {
                        bindUserToPlayerRequest.IsActive = true;
                    }

                    if (message.CaseInsensitiveContains("нет"))

                    {
                        bindUserToPlayerRequest.IsActive = false;
                    }

                    return bindUserToPlayerRequest;
                });

                Add("Игрок добавлен");
            }
        }

        private List<(int number, Guid userId, string Username)> GetNotBindedUsers()
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var users = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .Where(x => x.Role > EUserRole.UnregisteredUser);
                var players = _playerRepository.ListBySpecification(new UndeletedEntities<Player>());
                return users.Where(x => players.All(y => y.User.Id != x.Id))
                    .Select((x, index) => (index + 1, x.Id, x.FirstName + " " + x.LastName))
                    .ToList();
            }
        }

        protected override string CommandName => "Добавление игрока";

        public override void ProcessDialogEnded()
        {
            if (DialogData.UserId != Guid.Empty)
                _mediator.Send(DialogData);
        }
    }
}