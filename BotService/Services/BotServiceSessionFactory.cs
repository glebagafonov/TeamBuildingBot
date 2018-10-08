using System;
using System.Collections.Generic;
using Bot.Infrastructure.Mappings;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using NHibernate;
using NHibernate.Context;

namespace BotService.Services
{
    public class BotServiceSessionFactory : IAppSessionFactory
    {
        private readonly ISessionFactory _sessionFactory;

        public ISessionFactory SessionFactory => _sessionFactory;

        public BotServiceSessionFactory(ILogger logger)
        {
            var types = new List<Type>();
            types.AddRange(typeof(GameMapping).Assembly.GetTypes());
            
            _sessionFactory = SessionFactory<CallSessionContext>.CreateSessionFactory("systemDb", types, logger);
        }
    }
}