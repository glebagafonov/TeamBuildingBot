namespace Bot.Domain.Entities.Base
{
    public abstract class BaseAccount : Entity
    {
        public virtual BotUser User { get; set; }
    }
}