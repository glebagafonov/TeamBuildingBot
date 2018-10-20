using System.Linq.Expressions;

namespace Bot.Infrastructure.Specification.Base
{
    public abstract class SpecificationDefault<T> : ISpecification<T> where T : class
    {
        public Expression Expression { get; protected set; }

        public bool IsSatisfiedBy(T item)
        {
            throw new System.NotImplementedException();
        }
    }
}