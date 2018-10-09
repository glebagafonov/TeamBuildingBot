using Bot.Domain.Entities.Base;

namespace Bot.Domain.Entities
{
    public class TelegramAccount : BaseAccount
    {
        public virtual long TelegramId { get; set; }
    }
}