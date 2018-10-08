using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Infrastructure.Services.Interfaces;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace Bot.Infrastructure.Services
{
    public static class SessionFactory<TContext> 
        where TContext : ICurrentSessionContext
    {
        public static ISessionFactory CreateSessionFactory(string connectionName, IEnumerable<Type> types, ILogger logger)
        {
            var conf = new Configuration();
            
            conf.DataBaseIntegration(c =>
            {
//                c.
//                c.Dialect<SQLiteDialect>();
//                c.ConnectionStringName = connectionName;
                c.ConnectionStringName = connectionName;
                c.Driver<SQLite20Driver>();
                c.Dialect<SQLiteDialect>();
                c.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
            });
            var mapper = new ModelMapper();
            mapper.AddMappings(types);
            conf.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

            conf.CurrentSessionContext<TContext>();

            var shemaUpdate = new SchemaUpdate(conf);
            shemaUpdate.Execute(true, true);
            if (shemaUpdate.Exceptions.Any())
            {
                foreach (var exception in shemaUpdate.Exceptions)
                {
                    logger.Error(exception);
                }
                throw new AggregateException(shemaUpdate.Exceptions);
            }

            return conf.BuildSessionFactory();
        }
    }
}