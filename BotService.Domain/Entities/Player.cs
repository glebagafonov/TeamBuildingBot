namespace Bot.Domain.Entities
{
    public class Player : Entity
    {
        public virtual BotUser User               { get; set; }
        public virtual int     SkillValue         { get; set; }
        public virtual int     ParticipationRatio { get; set; }
        public virtual bool    IsActive           { get; set; }
    }
}