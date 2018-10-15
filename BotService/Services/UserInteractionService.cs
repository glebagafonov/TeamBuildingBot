using System;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Model.Dialog.Interfaces;
using BotService.Model.Dialogs;
using BotService.Requests;
using BotService.Services.Interfaces;
using MediatR;

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
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;

        public UserInteractionService(
            IDialogStorage dialogStorage,
            CommandFactory commandFactory,
            ILogger logger,
            IMediator mediator,
            IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository,
            IPlayerRepository playerRepository
        )
        {
            _dialogStorage                = dialogStorage;
            _commandFactory               = commandFactory;
            _logger                       = logger;
            _mediator                     = mediator;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _playerRepository             = playerRepository;
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
                    default:
                        throw new InvalidOperationException("Неизвестная команда");
                }
            }
        }

        private void ScheduleGameCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new ScheduleGameRequestDialog(_logger, _mediator).Start(communicator, user.Id,
                new ScheduleGameRequest(communicator));
            CreateDialog(dialog, user);
        }

        private void BindUserCommand(BotUser user, ICommunicator communicator)
        {
            IDialog dialog = new BindUserToPlayerDialog(_logger, _mediator, _threadContextSessionProvider,
                    _botUserRepository, _playerRepository)
                .Start(communicator, user.Id, new BindUserToPlayerRequest());
            CreateDialog(dialog, user);
        }

        private void CreateDialog(IDialog dialog, BotUser user)
        {
            dialog.CompleteEvent += CompleteEventHandler;
            _dialogStorage.SaveDialog(user, dialog);
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
            var dialog = new RegisterTelegramDialog(_logger, _mediator).Start(communicator, user.Id,
                new RegisterRequest(communicator));
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