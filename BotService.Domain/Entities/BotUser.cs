using Bot.Domain.Enums;

namespace Bot.Domain.Entities
{
    public class BotUser : Entity
    {
        public virtual int          TelegramId { get; set; }
        public virtual string       FirstName  { get; set; }
        public virtual string       LastName   { get; set; }
        public virtual EBotUserRole Role       { get; set; }
    }
}