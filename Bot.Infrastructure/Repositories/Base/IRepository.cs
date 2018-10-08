using System;
using System.Collections.Generic;
using Bot.Infrastructure.Specification;
using Bot.Infrastructure.Specification.Base;

namespace Bot.Infrastructure.Repositories.Base
{
    public interface IRepository<T> where T : class
    {
        T Get(Guid id);
        T GetBySpecification(ISpecification<T> spec);
        IEnumerable<T> ListBySpecification(ISpecification<T> spec);
        IEnumerable<TResult> ListByNHibernateSpecification<TResult>(INHibernateSpecification<T, TResult> spec);
        int CountBySpecification(ISpecification<T> spec);
        bool Save(T entity);
        bool Save(IEnumerable<T> entities);
        bool Delete(T entity);
        bool Delete(IEnumerable<T> entities);
    }
}
