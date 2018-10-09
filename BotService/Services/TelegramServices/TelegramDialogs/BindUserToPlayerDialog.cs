using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Model;
using BotService.Requests;
using BotService.Services.TelegramServices.TelegramDialogs.Base;
using MediatR;
using NHibernate.Util;

namespace BotService.Services.TelegramServices.TelegramDialogs
{
    public class BindUserToPlayerDialog : BaseTelegramDialog
    {
        private BindUserToPlayerRequest _request;
        private readonly long _chatId;
        private readonly IMediator _mediator;
        private readonly long _telegramId;
        private readonly TelegramInteractionService _telegramInteractionService;
        private readonly IBotUserRepository _botUserRepository;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;
        private List<BotUser> _notBindedPlayers;

        public BindUserToPlayerDialog(long chatId, long telegramId, IMediator mediator,
            TelegramInteractionService telegramInteractionService,
            ILogger logger,
            IBotUserRepository botUserRepository,
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository)
            : base(chatId,
                telegramInteractionService, logger)
        {
            _chatId                       = chatId;
            _telegramId                   = telegramId;
            _mediator                     = mediator;
            _telegramInteractionService   = telegramInteractionService;
            _botUserRepository            = botUserRepository;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository             = playerRepository;
            _request                      = new BindUserToPlayerRequest();
        }

        protected override string CommandName => "Добавление игрока";

        protected override async void Init()
        {
            Add("Введите номер пользователя^", x =>
            {
                if (!int.TryParse(x, out var number))
                {
                    throw new InvalidInputException("Введен не номер");
                }
                
                
                _request.User = _notBindedPlayers.ElementAtOrDefault(number - 1);
                if (_request.User == null)
                throw new InvalidInputException("Введен несуществующий номер");
            });
            Add("Введите коэффициент умения", x =>
                {
                    if (!int.TryParse(x, out var number))
                    {
                        throw new InvalidInputException("Введен не номер");
                    }

                    _request.SkillValue = number;
                }
            );
            Add("Введите коэффициент посещаемости", x =>
                {
                    if (!int.TryParse(x, out var number))
                    {
                        throw new InvalidInputException("Введен не номер");
                    }

                    _request.ParticipationRatio = number;
                }
            );
            Add("Введите активен ли игрок? (да/нет)", x =>
                {
                    if (!x.CaseInsensitiveContains("да") && !x.CaseInsensitiveContains("нет"))
                    {
                        throw new InvalidInputException("Введите \"да\" или \"нет\"");
                    }

                    if (x.CaseInsensitiveContains("да"))
                    {
                        _request.IsActive = true;
                    }

                    if (x.CaseInsensitiveContains("нет"))

                    {
                        _request.IsActive = false;
                    }
                }
            );
        }

        protected override async void ProcessResult()
        {
            await _mediator.Send(_request);
            await _telegramInteractionService.SendMessage(_chatId, "Пользователь успешно добавлен в команду");

            RaiseCompleteEvent();
        }

        protected override async void InitAction()
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                if (ActionContext.User == null)
                    throw new UnauthorizedException("Необходима авторизация");
                var users   = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>());
                var players = _playerRepository.ListBySpecification(new UndeletedEntities<Player>());
                int i       = 1;
                _notBindedPlayers = users.Where(x => players.All(y => y.User.Id != x.Id))
                    .ToList();
                if (!_notBindedPlayers.Any())
                {
                    await _telegramInteractionService.SendMessage(_chatId, "Все пользователи являются игроками");
                    RaiseCompleteEvent();
                }
                else
                {
                    var message = "Список пользователей:\n";
                    foreach (var notBindedPlayer in _notBindedPlayers)
                    {
                        message +=
                            $"{_notBindedPlayers.IndexOf(notBindedPlayer) + 1}. {notBindedPlayer.FirstName} {notBindedPlayer.LastName}\n";
                    }

                    await _telegramInteractionService.SendMessage(_chatId, message);
                    await _telegramInteractionService.SendMessage(_chatId, "Введите номер пользователя:");
                }
            }
        }
    }
}