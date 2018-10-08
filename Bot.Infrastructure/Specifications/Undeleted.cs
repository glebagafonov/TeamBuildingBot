using Bot.Domain.Entities;
using Bot.Infrastructure.Specification.Base;

namespace Bot.Infrastructure.Specifications
{
    public class Undeleted<T> : SpecificationDefault<T>
        where T : Entity
    {
        public Undeleted()
        {
            this.Expression = Specification<T>
                .Where(x => true)
//                .All()
                //.Where(x => x.State == EEntityState.Normal)
                .Expression;
        }
    }
}
