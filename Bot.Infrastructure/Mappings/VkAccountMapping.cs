using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Bot.Infrastructure.Mappings
{
    public class VkAccountMapping : UnionSubclassMapping<VkAccount>
    {
        public VkAccountMapping()
        {
            //Key(km => km.Column("Id"));
            this.Property(x => x.VkId);
        }
    }
}