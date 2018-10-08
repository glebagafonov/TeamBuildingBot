using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

namespace Bot.Infrastructure.Mappings
{
    public class BaseAccountMapping : BaseMapping<BaseAccount>
    {
        public BaseAccountMapping()
        {
            Table("`Account`");
            
            ManyToOne(x => x.User, c =>
            {
                c.Cascade(Cascade.None);
            });
        }
    }
}