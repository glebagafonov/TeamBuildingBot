using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class SortedPlayerMapping : BaseMapping<SortedPlayer>
    {
        public SortedPlayerMapping()
        {
            Table("`SortedPlayer`");
            this.Property(x => x.OrderNumber);
            ManyToOne(x => x.Player, c => { c.Cascade(Cascade.None); });
        }
    }
}