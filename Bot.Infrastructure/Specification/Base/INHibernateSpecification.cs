using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;

namespace Bot.Infrastructure.Specification.Base
{
    public interface INHibernateSpecification<T, TResult> where T : class
    {
        ICriterion Criterion { get; }
        DetachedCriteria DetachedCriteria { get; }
        QueryOverProjectionBuilder<T> Projections { get; }
    }

}