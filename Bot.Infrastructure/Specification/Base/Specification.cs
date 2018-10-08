// https://github.com/rjperes/DevelopmentWithADot.NHibernateSpecifications
// http://www.agile-code.com/blog/nhibernate-and-the-specification-pattern/

using System;
using System.Linq;
using System.Linq.Expressions;

namespace Bot.Infrastructure.Specification.Base
{
	public class Specification<T> : ISpecification<T> where T : class
	{
		private Func<T, bool> _compiled;

		protected internal Specification(Expression expression)
		{
			this.Expression = expression;
		}

		public static ISpecification<T> Create(Expression<Func<T, bool>> expression)
		{
			return (new Specification<T>(expression));
		}

		public static IQueryable<T> All()
		{
			return (SpecificationExtensions.All<T>());
		}

		public static IQueryable<T> Where(Expression<Func<T, bool>> condition)
		{
			return (SpecificationExtensions.Where(condition));
		}

		#region ISpecification<T> Members

		public bool IsSatisfiedBy(T item)
		{
			if (this._compiled == null)
			{
				this._compiled = SpecificationExtensions.ExtractCondition<T>(this.Expression).Compile();
			}

			return (this._compiled(item));
		}

		#endregion

		#region ISpecification Members

		public virtual Expression Expression
		{
			get;
			protected set;
		}

		#endregion

		#region Public override methods

		public override bool Equals(object obj)
		{
			var other = obj as Specification<T>;

			if ((other == null) || (other.GetType() != this.GetType()))
			{
				return (false);
			}

			if (ReferenceEquals(this, obj))
			{
				return (true);
			}

			return (this.Expression.Equals(other.Expression));
		}

		public override int GetHashCode()
		{
			return (this.Expression.GetHashCode());
		}

		public override string ToString()
		{
			return (this.Expression.ToString());
		}

		#endregion
	}
}
