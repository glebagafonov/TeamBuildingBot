// https://github.com/rjperes/DevelopmentWithADot.NHibernateSpecifications
// http://www.agile-code.com/blog/nhibernate-and-the-specification-pattern/

using System;
using System.Linq.Expressions;

namespace Bot.Infrastructure.Specification
{
    public interface ISpecification
    {
        Expression Expression
        {
            get;
        }
    }

    public interface ISpecification<in T> : ISpecification where T : class
    {
        Boolean IsSatisfiedBy(T item);
    }
}