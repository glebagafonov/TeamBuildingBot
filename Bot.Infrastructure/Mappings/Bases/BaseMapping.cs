using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Bot.Infrastructure.Mappings.Bases
{
    public class BaseMapping<T> : ClassMapping<T>
        where T: Entity
    {
        public BaseMapping()
        {
            Id(x => x.Id, m => m.Generator(Generators.Assigned));
            //this.PropertyDateTime(x => x.Timestamp);
            //this.PropertyEnum(x => x.State);
        }
    }
}
