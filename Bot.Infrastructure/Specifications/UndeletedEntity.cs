using System;
using Bot.Domain.Entities;
using Bot.Infrastructure.Specification.Base;

namespace Bot.Infrastructure.Specifications
{
    public class UndeletedEntity<T> : SpecificationDefault<T>
        where T : Entity
    {
        public UndeletedEntity(Guid id)
        {
            this.Expression = Specification<T>
//                .Where(x => x.State == EEntityState.Normal)
                .Where(x => x.Id == id)
                .Expression;
        }
    }
}
