namespace BotService.Services
{
    public static class Instruction
    {
        public static string GetInstruction =>
            "Привет! Я бот, который собирает людей на футбольчик!\r\n\r\nЯ знаю следующие команды:\r\n	Команда отмена диалога:\r\n"                                               +
            "		/cancel - Отмена диалога\r\n		\r\n	Команды помощи:\r\n		/help - Справка по боту\r\n		/commands - Список "                                           +
            "доступных команд\r\n\r\n	Команды действия над аккаунтом:\r\n		/register - Регистрация\r\n		/auth - Авторизация нового аккаунта\r\n"                               +
            "	Команды решения участия в игре:\r\n		/accept - Принять игру\r\n		/reject - Отказаться от игры\r\n\r\n	Команды статуса игрока:\r\n"                           +
            "		/inactive - Поставить статус не активен\r\n		/active - Поставить статус активен\r\n		/status - Узнать статус\r\n		\r\n"                               +
            "Теперь подробнее о командах:\r\n/register - Я начну с тобой диалог. Сначала спрошу имя, фамилию. Затем тебе надо будет ввести логин, пароль. "                         +
            "Все как в обычных регистрациях. Далее начинается интересное.\r\n/auth - Если ты зарегистрировался, например, с телеграма и пишешь с VK, то "                           +
            "тебе нужно присоединить аккаунт VK к твоему пользователю. Сделать это можно с помощью команды - /auth. Вводишь логин и пароль. Если все успешно, "                     +
            "то придет подтверждение.\r\n\r\nПосле регистрации напиши Глебу, он добавит тебя в команду и ты сможешь участвовать в играх.\r\n\r\nКогда ты будешь нужен "             +
            "команде, то я задам тебе вопрос -Играешь?-. Ты можешь ответить -Да-, -Нет- или отметить диалог командой - /cancel. Но учти тебе дается некоторое время "               +
            "на подтверждение.\r\nЕсли ты отменил диалог, то можешь воспользоваться командами /accept и /reject. Я дам тебе выбрать игру для твоего решения.\r\n\r\nПро статусы:"   +
            "\r\nЕсть два статуса: Активен и Неактивен\r\nСтатус -Активен- значит, что я буду высылать тебе запрос на участие. Соответственно -Неактивен- означает, что я оставлю " +
            "тебя в покое.\r\nИзменить статус можно команадами: /active и /inactive.\r\nПолучить свой статус можно с помощью команды - /status\r\n";
    }
}