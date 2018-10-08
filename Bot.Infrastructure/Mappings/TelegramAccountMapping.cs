using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Bot.Infrastructure.Mappings
{
    public class TelegramAccountMapping : UnionSubclassMapping<TelegramAccount>
    {
        public TelegramAccountMapping()
        {
            this.Property(x => x.TelegramId);
        }
    }
}