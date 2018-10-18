using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class PlayerEventMapping : BaseMapping<PlayerEvent>
    {
        public PlayerEventMapping()
        {
            Table("`PlayerEvent`");
            this.PropertyDateTime(x => x.EventTime);
            ManyToOne(x => x.Player, c => { c.Cascade(Cascade.None); });
        }
    }
}