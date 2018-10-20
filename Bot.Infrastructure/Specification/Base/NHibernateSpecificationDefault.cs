using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;

namespace Bot.Infrastructure.Specification.Base
{
    public abstract class NHibernateSpecificationDefault<T, TResult> : INHibernateSpecification<T, TResult> where T : class
    {
        public ICriterion Criterion { get; protected set; }
        public DetachedCriteria DetachedCriteria { get; protected set; }

        public QueryOverProjectionBuilder<T> Projections { get; protected set; }
    }
}