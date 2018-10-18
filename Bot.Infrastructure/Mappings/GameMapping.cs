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

            Set(x => x.SortedPlayersByRating, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_SortedPlayer");
            }, r => r.ManyToMany(m => m.Column("PlayerId")));
            
            Set(x => x.AcceptedPlayers, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_Player");
            }, r => r.ManyToMany(m => m.Column("PlayerId")));
            
            Set(x => x.DeclinedPlayers, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_Player");
            }, r => r.ManyToMany(m => m.Column("PlayerId")));
            
            Set(x => x.RequestedPlayers, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_PlayerEvent");
            }, r => r.ManyToMany(m => m.Column("PlayerEventId")));
            
            Set(x => x.DistributedPlayers, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("GameId"));
                c.Table("Game_DistributedPlayers");
            }, r => r.ManyToMany(m => m.Column("PlayerId")));
        }
    }
}