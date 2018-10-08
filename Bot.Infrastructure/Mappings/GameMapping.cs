using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class GameMapping : BaseMapping<Game>
    {
        public GameMapping()
        {
            Table("`Game`");
            this.PropertyDateTime(x => x.DateTime);

            Set(x => x.Players, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_Player");
            }, r => r.ManyToMany(m => m.Column("PlayerId")));
        }
    }
}