using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specification;
using Bot.Infrastructure.Specification.Base;
using NHibernate;
using NHibernate.Transform;

namespace Bot.Infrastructure.Repositories.Base
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ISessionProvider _provider;
        private readonly ILogger _logger;

        protected ISession Session => _provider.Session;

        public Repository(ISessionProvider provider, ILogger logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public virtual T Get(Guid id)
        {
            try
            {
                T result;
                ProcessSQLCommandWithRetries<T, T>(null, c => Session.Get<T>(id), out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести получение данных данных ({e.Message})", e);
            }
        }

        public virtual T GetBySpecification(ISpecification<T> spec)
        {
            try
            {
                T result;
                ProcessSQLCommandWithRetries<T, T>(null, c => Session.QueryBySpecification(spec).FirstOrDefault(), out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести получение данных данных ({e.Message})", e);
            }
        }

        public IEnumerable<T> ListBySpecification(ISpecification<T> spec)
        {
            try
            {
                IEnumerable<T> result;
                ProcessSQLCommandWithRetries<T, IEnumerable<T>>(null, c => Session.QueryBySpecification(spec).ToList(), out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести получение данных данных ({e.Message})", e);
            }
        }

        public IEnumerable<TResult> ListByNHibernateSpecification<TResult>(INHibernateSpecification<T, TResult> spec)
        {
            try
            {
                IEnumerable<TResult> result;
                ProcessSQLCommandWithRetries<T, IEnumerable<TResult>>(null, c =>
                {
                    
                    if (spec.DetachedCriteria != null)
                        return spec.DetachedCriteria.GetExecutableCriteria(Session).SetResultTransformer(Transformers.AliasToBean<TResult>()).List<TResult>();
                    return
                        Session.QueryOver<T>()
                            .Where(spec.Criterion)
                            .SelectList(x => spec.Projections)
                            .TransformUsing(Transformers.AliasToBean<TResult>())
                            .List<TResult>();
                }, out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести получение данных данных ({e.Message})", e);
            }
        }

        public int CountBySpecification(ISpecification<T> spec)
        {
            try
            {
                int result;
                ProcessSQLCommandWithRetries<T, int>(null, x => Session.QueryBySpecification(spec).Count(), out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести получение данных данных ({e.Message})", e);
            }
        }

        public virtual bool Save(T entity)
        {
            try
            {
                bool result;
                ProcessSQLCommandWithRetries(entity, x =>
                {
                    using (var scope = new TransactionScope(
                        TransactionScopeOption.Required, 
                        new TransactionOptions { Timeout = TimeSpan.FromMinutes(10)/*, IsolationLevel = IsolationLevel.ReadUncommitted*/}))
                    {
                        Session.SaveOrUpdate(x);
                        scope.Complete();
                    }
                    return true;
                }, out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести сохранение данных ({e.Message})", e);
            }
        }

        public virtual bool Save(IEnumerable<T> entities)
        {
            try
            {
                bool result;
                ProcessSQLCommandWithRetries(entities, x =>
                {
                    using (var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { Timeout = TimeSpan.FromMinutes(10)}))
                    {
                        foreach (var entity in x)
                            Session.SaveOrUpdate(entity);

                        scope.Complete();
                    }
                    return true;
                }, out result);

                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось произвести сохранение данных ({e.Message})", e);
            }
        }

        public virtual bool Delete(T entity)
        {
            try
            {
                bool result;
                ProcessSQLCommandWithRetries(entity, x =>
                {
                    using (var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { Timeout = TimeSpan.FromMinutes(10)}))
                    {
                        Session.Delete(x);
                        scope.Complete();
                    }
                    return true;
                }, out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось удалить элемент ({e.Message})", e);
            }
        }

        public virtual bool Delete(IEnumerable<T> entities)
        {
            try
            {
                bool result;
                ProcessSQLCommandWithRetries(entities, x =>
                {
                    using (var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { Timeout = TimeSpan.FromMinutes(10)}))
                    {
                        foreach (var entity in x)
                            Session.Delete(entity);

                        scope.Complete();
                    }
                    return true;
                }, out result);
                return result;
            }
            catch (Exception e)
            {
                throw new HibernateException($"Не удалось удалить элементы ({e.Message})", e);
            }
        }

        private void ProcessSQLCommandWithRetries<TInput, TOutput>(TInput input, Func<TInput, TOutput> command, out TOutput result)
        {
            var retryCount = 7;

            while (!ProcessCommand(input, retryCount, command, out result))
            {
                retryCount--;
            }
        }


        private bool ProcessCommand<TInput, TOutput>(TInput entity, int retryCount, Func<TInput, TOutput> process, out TOutput result)
        {
            try
            {
                result = process(entity);
            }
            catch (Exception exception)
            {
                _logger?.Warn(exception);
                if (retryCount <= 0)
                {
                    throw;
                }
                var rnd = new Random();
                System.Threading.Thread.Sleep(50 + (int)Math.Round(rnd.NextDouble() * 300)); // random sleep for 50..350 ms
                result = default(TOutput);
                return false;
            }
            return true;
        }
    }
}
