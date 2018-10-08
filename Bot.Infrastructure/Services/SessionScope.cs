using System;
using Bot.Infrastructure.Services.Interfaces;
using NHibernate;

namespace ASUDD.Common.Infrastructure.Services
{
    public sealed class SessionScope : IDisposable
    {
        private readonly IThreadContextSessionProvider _sessionProvider;

        public ISession Session { get; set; }

        public SessionScope(IThreadContextSessionProvider sessionProvider)
        {
            _sessionProvider = sessionProvider;
            Session = _sessionProvider.OpenSession();
        }

        public void Dispose()
        {
            _sessionProvider.CloseSession();
        }
    }
}
