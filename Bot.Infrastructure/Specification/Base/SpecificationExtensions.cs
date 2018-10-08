// https://github.com/rjperes/DevelopmentWithADot.NHibernateSpecifications
// http://www.agile-code.com/blog/nhibernate-and-the-specification-pattern/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Linq;
using Expression = System.Linq.Expressions.Expression;

namespace Bot.Infrastructure.Specification.Base
{
    public static class SpecificationExtensions
    {
        #region Private static helper methods

        private static IQueryable<T> CreateQueryable<T>()
        {
            return (new NhQueryable<T>(((ISessionImplementor)ServiceLocator.Get<ISessionProvider>().Session)));
        }

        internal static Expression<Func<T, bool>> ExtractCondition<T>(Expression expression)
        {
            switch (expression)
            {
                case Expression<Func<T, bool>> expression1:
                    return expression1;
                case MethodCallExpression callExpression:
                    foreach (var argument in callExpression.Arguments)
                    {
                        var condition = ExtractCondition<T>(argument);

                        if (condition != null)
                        {
                            return (condition);
                        }
                    }

                    break;
                case UnaryExpression unaryExpression:
                    return (ExtractCondition<T>(unaryExpression.Operand));
                default:
                    break;
            }

            return (null);
        }

        internal static IEnumerable<Expression<Func<T, bool>>> ExtractAllConditions<T>(Expression expression)
        {
            switch (expression)
            {
                case Expression<Func<T, bool>> expression1:
                    yield return expression1;
                    break;
                case MethodCallExpression callExpression:
                    foreach (var argument in callExpression.Arguments)
                    {
                        var conditions = ExtractAllConditions<T>(argument);

                        foreach (var condition in conditions)
                        {
                            yield return condition;
                        }
                    }

                    break;
                case UnaryExpression unaryExpression:
                    var expressions = ExtractAllConditions<T>(unaryExpression.Operand);
                    foreach (var expr in expressions)
                    {
                        yield return expr;
                    }

                    break;
                default:
                    break;
            }
        }

        private static MethodCallExpression FindMethodCallExpression(Expression expression, Type type, string name)
        {
            if (!(expression is MethodCallExpression methodCallExp))
                return null;

            if ((methodCallExp.Method.DeclaringType == type) && (methodCallExp.Method.Name == name))
            {
                return (methodCallExp);
            }

            foreach (var argument in methodCallExp.Arguments)
            {
                methodCallExp = FindMethodCallExpression(argument, type, name);

                if (methodCallExp != null)
                {
                    return (methodCallExp);
                }
            }

            return (null);
        }

        private static Expression<Func<T, object>> ExtractOrder<T>(Expression expression, string orderKind)
        {
            var methodCallExp = FindMethodCallExpression(expression, typeof(Queryable), orderKind);

            if (methodCallExp == null)
                return null;

            var lambda = (methodCallExp.Arguments.Last() as UnaryExpression).Operand as LambdaExpression;

            lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(lambda.Body, typeof(object)), lambda.Name, lambda.TailCall, lambda.Parameters);

            return (lambda as Expression<Func<T, object>>);

        }

        private static Expression<Func<T, object>> ExtractOrderBy<T>(Expression expression)
        {
            return (ExtractOrder<T>(expression, "OrderBy"));
        }

        private static Expression<Func<T, object>> ExtractOrderByDescending<T>(Expression expression)
        {
            return (ExtractOrder<T>(expression, "OrderByDescending"));
        }

        public static Expression<Func<T, object>> ExtractThenBy<T>(Expression expression)
        {
            return (ExtractOrder<T>(expression, "ThenBy"));
        }

        public static Expression<Func<T, object>> ExtractThenByDescending<T>(Expression expression)
        {
            return (ExtractOrder<T>(expression, "ThenByDescending"));
        }

        private static int ExtractPaging(Expression expression, string pagingKind)
        {
            var methodCallExp = FindMethodCallExpression(expression, typeof(Queryable), pagingKind);

            if (methodCallExp != null)
            {
                return ((int)(methodCallExp.Arguments.Last() as ConstantExpression).Value);
            }

            return (0);
        }

        private static int ExtractTake(Expression expression)
        {
            return (ExtractPaging(expression, "Take"));
        }

        private static int ExtractSkip(Expression expression)
        {
            return (ExtractPaging(expression, "Skip"));
        }

        public static Expression<Func<T, object>> ExtractFetch<T>(Expression expression)
        {
            var methodCallExp = FindMethodCallExpression(expression, typeof(EagerFetchingExtensionMethods), "Fetch");

            return (methodCallExp?.Arguments.Last() as UnaryExpression)?.Operand as Expression<Func<T, object>>;
        }

        public static IEnumerable<Expression<Func<T, object>>> ExtractAllFetches<T>(Expression expression)
        {
            switch (expression)
            {
                case MethodCallExpression callExpression:
                    if (callExpression.Method.DeclaringType == typeof(EagerFetchingExtensionMethods) &&
                        callExpression.Method.Name == "Fetch")
                    {
                        yield return (callExpression.Arguments.Last() as UnaryExpression)?.Operand as Expression<Func<T, object>>;
                    }
                    foreach (var argument in callExpression.Arguments)
                    {
                        var fetches = ExtractAllFetches<T>(argument);

                        foreach (var fetch in fetches)
                        {
                            yield return fetch;
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        private static IQueryable<T> AddFetching<T>(IQueryable<T> queryable, Expression source, bool skipOrdering, bool skipPaging)
        {
            if (!skipOrdering)
            {
                queryable = AddOrdering(queryable, source, true);
            }

            if (!skipPaging)
            {
                queryable = AddPaging(queryable, source, false);
            }

            var fetches = ExtractAllFetches<T>(source);

            foreach (var fetch in fetches)
            {
                queryable = queryable.Fetch(fetch);
            }

            return (queryable);
        }

        private static IQueryable<T> AddPaging<T>(IQueryable<T> queryable, Expression source, bool skipOrdering)
        {
            var take = ExtractTake(source);
            var skip = ExtractSkip(source);

            if (!skipOrdering)
            {
                queryable = AddOrdering(queryable, source, true);
            }

            if (skip != 0)
            {
                queryable = queryable.Skip(skip);
            }

            if (take != 0)
            {
                queryable = queryable.Take(take);
            }

            return (queryable);
        }

        private static IQueryable<T> AddOrdering<T>(IQueryable<T> queryable, Expression source, bool skipPaging)
        {
            var orderBy = ExtractOrderBy<T>(source);
            var orderByDescending = ExtractOrderByDescending<T>(source);
            var thenBy = ExtractThenBy<T>(source);
            var thenByDescending = ExtractThenByDescending<T>(source);

            if (orderBy != null)
            {
                queryable = queryable.OrderBy(orderBy);
            }

            if (orderByDescending != null)
            {
                queryable = queryable.OrderByDescending(orderByDescending);
            }

            if (thenBy != null)
            {
                queryable = (queryable as IOrderedQueryable<T>).ThenBy(thenBy);
            }

            if (thenByDescending != null)
            {
                queryable = (queryable as IOrderedQueryable<T>).ThenByDescending(thenByDescending);
            }

            if (!skipPaging)
            {
                queryable = AddPaging(queryable, source, true);
            }

            return (queryable);
        }

        #endregion

        #region Public extension methods

        public static IQueryable All(this Type type)
        {
            return (typeof(SpecificationExtensions).GetMethod("All", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, null) as IQueryable);
        }

        public static IQueryable<T> All<T>() where T : class
        {
            return (Where<T>(x => true));
        }

        public static IQueryable<T> Where<T>(Expression<Func<T, bool>> condition) where T : class
        {
            return (CreateQueryable<T>().Where(condition));
        }

        public static IQueryable<T> QueryBySpecification<T>(this ISession session, ISpecification<T> specification) where T : class
        {
            var conditions = ExtractAllConditions<T>(specification.Expression).ToList();
            var queryable = session.Query<T>();
            foreach (var condition in conditions)
                queryable = queryable.Where(condition);

            queryable = AddOrdering(queryable, specification.Expression, false);
            queryable = AddFetching(queryable, specification.Expression, true, true);
            return (queryable);
        }

        public static ISpecification<T> And<T>(this ISpecification<T> specification, IEnumerable<Expression<Func<T, bool>>> conditions) where T : class
        {
            var queryable = CreateQueryable<T>();
            var firstConditions = ExtractAllConditions<T>(specification.Expression).ToList();
            foreach (var condition in firstConditions)
                queryable = queryable.Where(condition);

            var secondConditions = conditions;
            foreach (var condition in secondConditions)
                queryable = queryable.Where(condition);

            queryable = AddOrdering(queryable, specification.Expression, false);
            queryable = AddPaging(queryable, specification.Expression, false);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> And<T>(this ISpecification<T> specification, ISpecification<T> other) where T : class
        {
            return (And(specification, ExtractAllConditions<T>(other.Expression).ToList()));
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> specification, IEnumerable<Expression<Func<T, bool>>> conditions) where T : class
        {
            var queryable = CreateQueryable<T>();
            var firstConditions = ExtractAllConditions<T>(specification.Expression).ToList();
            foreach (var condition in firstConditions)
                queryable = queryable.Where(condition);

            var secondConditions = conditions;
            foreach (var condition in secondConditions)
                queryable = queryable.Where(condition);

            queryable = AddOrdering(queryable, specification.Expression, false);
            queryable = AddPaging(queryable, specification.Expression, false);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> specification, ISpecification<T> other) where T : class
        {
            return (Or(specification, ExtractAllConditions<T>(other.Expression).ToList()));
        }

        public static ISpecification<T> Not<T>(this ISpecification<T> specification) where T : class
        {
            var not = Expression.Not(ExtractCondition<T>(specification.Expression).Body);

            var queryable = CreateQueryable<T>().Where(Expression.Lambda<Func<T, bool>>(not, ExtractCondition<T>(specification.Expression).Parameters));
            queryable = AddOrdering(queryable, specification.Expression, false);
            queryable = AddPaging(queryable, specification.Expression, false);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> AsSpecification<T>(this Expression<Func<T, bool>> expression) where T : class
        {
            return (Specification<T>.Create(expression));
        }

        public static Expression<Func<T, bool>> AsCondition<T>(this ISpecification<T> specification) where T : class
        {
            return (ExtractCondition<T>(specification.Expression));
        }

        public static ISpecification<T> Take<T>(this ISpecification<T> specification, int count) where T : class
        {
            var queryable = CreateQueryable<T>();
            var conditions = ExtractAllConditions<T>(specification.Expression).ToList();
            foreach (var condition in conditions)
                queryable = queryable.Where(condition);

            queryable = queryable.Take(count);

            var skip = ExtractSkip(specification.Expression);

            if (skip != 0)
            {
                queryable = queryable.Skip(skip);
            }

            queryable = AddOrdering(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> Skip<T>(this ISpecification<T> specification, int count) where T : class
        {
            var queryable = CreateQueryable<T>();
            var conditions = ExtractAllConditions<T>(specification.Expression).ToList();
            foreach (var condition in conditions)
                queryable = queryable.Where(condition);

            queryable = queryable.Skip(count);

            var take = ExtractTake(specification.Expression);

            if (take != 0)
            {
                queryable = queryable.Take(take);
            }

            queryable = AddOrdering(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> OrderBy<T>(this ISpecification<T> specification, Expression<Func<T, object>> orderBy) where T : class
        {
            var queryable = CreateQueryable<T>();
            queryable = queryable.OrderBy(orderBy).Where(ExtractCondition<T>(specification.Expression));
            queryable = AddPaging(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> OrderByDescending<T>(this ISpecification<T> specification, Expression<Func<T, object>> orderByDescending) where T : class
        {
            var queryable = CreateQueryable<T>();
            queryable = queryable.OrderByDescending(orderByDescending).Where(ExtractCondition<T>(specification.Expression));
            queryable = AddPaging(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> ThenBy<T>(this ISpecification<T> specification, Expression<Func<T, object>> orderBy) where T : class
        {
            var queryable = CreateQueryable<T>();
            queryable = AddOrdering(queryable, specification.Expression, true);
            queryable = (queryable as IOrderedQueryable<T>).ThenBy(orderBy).Where(ExtractCondition<T>(specification.Expression));
            queryable = AddPaging(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> ThenByDescending<T>(this ISpecification<T> specification, Expression<Func<T, object>> orderBy) where T : class
        {
            var queryable = CreateQueryable<T>();
            queryable = AddOrdering(queryable, specification.Expression, true);
            queryable = (queryable as IOrderedQueryable<T>).ThenByDescending(orderBy).Where(ExtractCondition<T>(specification.Expression));
            queryable = AddPaging(queryable, specification.Expression, true);

            return (new Specification<T>(queryable.Expression));
        }

        public static ISpecification<T> Fetch<T>(this ISpecification<T> specification, Expression<Func<T, object>> path) where T : class
        {
            var queryable = CreateQueryable<T>();
            var conditions = ExtractAllConditions<T>(specification.Expression).ToList();
            foreach (var condition in conditions)
                queryable = queryable.Where(condition);
            queryable = AddOrdering(queryable, specification.Expression, false);
            queryable = AddPaging(queryable, specification.Expression, false);
            queryable = AddFetching(queryable, specification.Expression, true, true);
            queryable = queryable.Fetch(path);

            return (new Specification<T>(queryable.Expression));
        }

        #endregion
    }
}