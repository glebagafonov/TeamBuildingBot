using Bot.Domain.Entities.Base;

namespace Bot.Domain.Entities
{
    public class TelegramAccount : BaseAccount
    {
        public virtual int TelegramId { get; set; }
    }
}