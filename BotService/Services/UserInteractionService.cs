using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.Dialog.Interfaces;
using BotService.Model.Dialogs;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using MediatR;
using NHibernate.Util;

namespace BotService.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly IBotUserRepository _botUserRepository;
        private readonly CommandFactory _commandFactory;
        private readonly IDialogStorage _dialogStorage;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly IPlayerRepository _playerRepository;
        private readonly IServiceConfiguration _serviceConfiguration;
        private readonly IGameRepository _gameRepository;
        private readonly ICommunicatorFactory _communicatorFactory;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;

        public UserInteractionService(
            IDialogStorage dialogStorage,
            CommandFactory commandFactory,
            ILogger logger,
            IMediator mediator,
            IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository,
            IPlayerRepository playerRepository,
            IServiceConfiguration serviceConfiguration,
            IGameRepository gameRepository,
            ICommunicatorFactory communicatorFactory
        )
        {
            _dialogStorage                = dialogStorage;
            _commandFactory               = commandFactory;
            _logger                       = logger;
            _mediator                     = mediator;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _playerRepository             = playerRepository;
            _serviceConfiguration         = serviceConfiguration;
            _gameRepository = gameRepository;
            _communicatorFactory = communicatorFactory;
        }

        public void ProcessMessage(BotUser user, ICommunicator communicator, string message)
        {
            try
            {
                var command = _commandFactory.GetCommand(message, user);
                if (command.HasValue)
                    ProcessCommandMessage(user, communicator, command.Value);
                else
                    ProcessSimpleMessage(user, communicator, message);
            }
            catch (UnauthorizedException exception)
            {
                communicator.SendMessage("У вас нет прав для выполнения этой команды");
                _logger.Error(exception);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void ProcessSimpleMessage(BotUser user, ICommunicator communicator, string message)
        {
            var dialog = _dialogStorage.Get(user);
            if (dialog != null)
                dialog.ProcessMessage(message, communicator);
            else
                communicator.SendMessage("Не понимаю тебя. Введи команду или воспользуйся командой - /help");
        }

        private void ProcessCommandMessage(BotUser user, ICommunicator communicator, ECommandType command)
        {
            var dialog = _dialogStorage.Get(user);
            if (dialog != null)
            {
                if (command == ECommandType.Cancel)
                {
                    dialog = null;
                    _dialogStorage.RemoveDialog(user);
                    communicator.SendMessage(
                        "Диалог отменен. Введи команду. Если нужна помошь воспользуйся командой - /help");
                }
                else
                {
                    communicator.SendMessage("Ответь на предыдущее сообщение или отмени диалог командой - /cancel");
                }
            }
            else
            {
                switch (command)
                {
                    case ECommandType.Register:
                    {
                        RegisterCommand(user, communicator);
                    }
                        break;
                    case ECommandType.Cancel:
                    {
                        communicator.SendMessage(
                            "Нет диалога для отмены. Введи команду. Если нужна помошь воспользуйся командой - /help");
                    }
                        break;
                    case ECommandType.Help:
                    {
                        HelpCommand(user, communicator);
                    }
                        break;
                    case ECommandType.Authorize:
                    {
                        communicator.SendMessage(
                            "Команда в разработке. Введи другую команду. Если нужна помошь воспользуйся командой - /help");
                    }
                        break;
                    case ECommandType.BindPlayer:
                    {
                        BindUserCommand(user, communicator);
                    }
                        break;
                    case ECommandType.ScheduleGame:
                    {
                        ScheduleGameCommand(user, communicator);
                    }
                        break;
                    case ECommandType.CancelGame:
                    {
                        CancelGameCommand(user, communicator);
                    }
                        break;
                    case ECommandType.AddPlayerToGame:
                    {
                        AddPlayerToGameCommand(user, communicator);
                    }
                        break;
                    case ECommandType.Accept:
                    {
                        PlayerGameDeferedDecisionCommand(user, communicator, true);
                    }
                        break;
                    case ECommandType.Reject:
                    {
                        PlayerGameDeferedDecisionCommand(user, communicator, false);
                    }
                        break;
                    default:
                        throw new InvalidOperationException("Неизвестная команда");
                }
            }
        }

        public void SendMessage(string text, BotUser user)
        {
            foreach (var x in user.UserAccounts.Select(x => _communicatorFactory.GetCommunicator(x)))
                x.SendMessage(text);
        }

        public void StartGameConfirmationDialog(Player player, List<ICommunicator> communicators, Guid gameId)
        {
            SetNewDialog(player.User, communicators,
                new PlayerGameDecisionRequestDialog(communicators, player.User.Id,
                    new PlayerGameDecisionRequest() {GameId = gameId, PlayerId = player.Id}, _logger, _mediator,
                    _threadContextSessionProvider, _gameRepository));
        }

        public void StopDialog(BotUser playerUser)
        {
            StopCurrentDialog(playerUser, playerUser.UserAccounts.Select(x => _communicatorFactory.GetCommunicator(x)));
        }

        private void SetNewDialog(BotUser user, IEnumerable<ICommunicator> communicators, IDialog dialog)
        {
            StopCurrentDialog(user, communicators);
            CreateDialog(dialog, user);
        }

        private void StopCurrentDialog(BotUser user, IEnumerable<ICommunicator> communicators)
        {
            var currentDialog = _dialogStorage.Get(user);
            if (currentDialog != null)
            {
                _dialogStorage.RemoveDialog(user);
                currentDialog.CompleteEvent -= CompleteEventHandler;
                foreach (var x in communicators)
                    x.SendMessage("Текущий диалог прерван.");
            }
        }
        
        private void AddPlayerToGameCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new AddPlayerToGameRequestDialog(new List<ICommunicator>(){communicator}, user.Id,
                new AddPlayerToGameRequest(), _logger, _mediator, _threadContextSessionProvider, _gameRepository, _serviceConfiguration);
            CreateDialog(dialog, user);
        }
        
        private void PlayerGameDeferedDecisionCommand(BotUser user, ICommunicator communicator, bool decision)
        {
            IDialog dialog = new PlayerGameDeferedDecisionRequestDialog(new List<ICommunicator>(){communicator}, user.Id,
                new PlayerGameDecisionRequest(), _logger, _mediator, _threadContextSessionProvider, _gameRepository, _serviceConfiguration, decision);
            CreateDialog(dialog, user);
        }
        
        private void CancelGameCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new CancelGameRequestDialog(new List<ICommunicator>(){communicator}, user.Id,
                new CancelGameRequest(), _logger, _mediator, _threadContextSessionProvider, _gameRepository, _serviceConfiguration);
            CreateDialog(dialog, user);
        }

        private void ScheduleGameCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new ScheduleGameRequestDialog(new List<ICommunicator>(){communicator}, user.Id,
                new ScheduleGameRequest(communicator), _logger, _mediator, _serviceConfiguration);
            CreateDialog(dialog, user);
        }

        private void BindUserCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new BindUserToPlayerDialog(new List<ICommunicator>(){communicator}, user.Id, new BindUserToPlayerRequest(), _logger,
                    _mediator, _threadContextSessionProvider,
                    _botUserRepository, _playerRepository);
            CreateDialog(dialog, user);
        }

        private void CreateDialog(IDialog dialog, BotUser user)
        {
            dialog.CompleteEvent += CompleteEventHandler;
            _dialogStorage.SaveDialog(user, dialog);
            dialog.Start();
        }

        private void HelpCommand(BotUser user, ICommunicator communicator)
        {
            var commands = _commandFactory.GetCommandsForUser(user);
            if (commands.Count > 0)
                communicator.SendMessage(
                    $"Список доступных команд:\n{string.Join("\n", commands.Select(x => $"{x.command} - {x.description}"))}");
            else
                communicator.SendMessage("Нет доступных команд");
        }

        private void RegisterCommand(BotUser user, ICommunicator communicator)
        {
            var dialog = new RegisterTelegramDialog(new List<ICommunicator>(){communicator}, user.Id,
                new RegisterRequest(communicator), _logger, _mediator);
            CreateDialog(dialog, user);
        }

        private void CompleteEventHandler(Guid userId)
        {
            var dialog = _dialogStorage.RemoveDialog(userId);
            if (dialog != null)
                dialog.CompleteEvent -= CompleteEventHandler;
        }
    }
}