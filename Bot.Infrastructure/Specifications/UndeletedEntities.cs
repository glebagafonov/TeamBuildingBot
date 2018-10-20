using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Domain.Entities;
using Bot.Infrastructure.Specification.Base;

namespace Bot.Infrastructure.Specifications
{
    public class UndeletedEntities<T> : SpecificationDefault<T>
        where T : Entity
    {
        public UndeletedEntities()
        {
            this.Expression = Specification<T>
                .Where(x => true)
//                .Where(x => x.State == EEntityState.Normal)
                .Expression;
        }

        public UndeletedEntities(IEnumerable<Guid> ids)
        {
            this.Expression = Specification<T>
//                .Where(x => x.State == EEntityState.Normal)
                .Where(x => ids.Contains(x.Id))
                .Expression;
        }
    }
}
