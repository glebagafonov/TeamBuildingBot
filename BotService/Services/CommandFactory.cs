using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;

namespace BotService.Services
{
    public class CommandFactory
    {
        private readonly Dictionary<string, Command> _commands;

        public CommandFactory()
        {
            _commands = new Dictionary<string, Command>();
        }

        public void Initialize()
        {
            Add("/help", ECommandType.Help, EUserRole.UnregisteredUser, "Список доступных команд");

            Add("/register", ECommandType.Register, EUserRole.UnregisteredUser, "Регистрация");
            Add("/cancel", ECommandType.Cancel, EUserRole.UnregisteredUser, "Отмена диалога");
            Add("/auth", ECommandType.Authorize, EUserRole.UnregisteredUser, "Авторизация нового аккаунта");

            Add("/binduser", ECommandType.BindPlayer, EUserRole.Moderator, "Добавление игрока из пользователей");
            
            Add("/plangame", ECommandType.ScheduleGame, EUserRole.Moderator, "Запланировать игру");
            Add("/cancelgame", ECommandType.CancelGame, EUserRole.Moderator, "Отменить игру");
            
            
            Add("/accept", ECommandType.Accept, EUserRole.Moderator, "Принять игру");
            Add("/reject", ECommandType.Accept, EUserRole.Moderator, "Отказаться от игры");
        }

        private void Add(string command, ECommandType commandType, EUserRole minimalUserAccessRole, string description)
        {
            _commands.Add(command, new Command(commandType, command, minimalUserAccessRole, description));
        }

        public ECommandType? GetCommand(string message, BotUser user)
        {
            if (_commands.ContainsKey(message))
            {
                if (user.Role < _commands[message].MinimalAccessRole)
                    throw new UnauthorizedException();
                return _commands[message].CommandType;
            }

            return null;
        }

        public List<(string command, string description)> GetCommandsForUser(BotUser user)
        {
            return _commands.Values.Where(x => x.MinimalAccessRole <= user.Role)
                .Select(x => (x.CommandMessage, x.Description)).ToList();
        }

        private class Command
        {
            public ECommandType CommandType       { get; }
            public string       CommandMessage    { get; }
            public EUserRole    MinimalAccessRole { get; }
            public string       Description       { get; }

            public Command(ECommandType commandType, string commandMessage, EUserRole minimalAccessRole,
                string description)
            {
                CommandType       = commandType;
                CommandMessage    = commandMessage;
                MinimalAccessRole = minimalAccessRole;
                Description       = description;
            }
        }
    }
}