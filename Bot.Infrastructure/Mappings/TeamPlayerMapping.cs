using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class TeamPlayerMapping : BaseMapping<TeamPlayer>
    {
        public TeamPlayerMapping()
        {
            Table("`TeamPlayer`");
            this.Property(x => x.TeamNumber);
            ManyToOne(x => x.Player, c => { c.Cascade(Cascade.None); });
        }
    }
}