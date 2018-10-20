using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class PlayerMapping : BaseMapping<Player>
    {
        public PlayerMapping()
        {
            Table("`Player`");
            this.Property(x => x.ParticipationRatio);
            this.Property(x => x.SkillValue);
            this.Property(x => x.IsActive);
            ManyToOne(x => x.User, c =>
            {
                c.Cascade(Cascade.None);
            });
        }
    }
}