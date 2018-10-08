using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ASUDD.Common.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using NHibernate;

namespace Bot.Infrastructure.Services
{
    public abstract class ThreadContextSessionProvider : IThreadContextSessionProvider
    {
        private readonly ILogger _logger;

        private class SessionInfo
        {
            public ISession Session { get; set; }
            public int ReferenceCount { get; set; }
            public int ContextId { get; set; }
        }

        private readonly List<SessionInfo> _sessions;
        private readonly Object _lockObj;

        public ISessionFactory SessionFactory { get; protected set; }

        public ISession Session
        {
            get
            {
                var currentContext = Thread.CurrentThread.GetHashCode();
                SessionInfo sessionInfo;
                lock (_lockObj)
                {
                    sessionInfo = _sessions.FirstOrDefault(x => x.ContextId == currentContext);
                }
                if (sessionInfo == null)
                    throw new InvalidOperationException("no session bound to current context and current source");

                return sessionInfo.Session;
            }
        }

        protected ThreadContextSessionProvider(ILogger logger, IAppSessionFactory sessionFactory)
        {
            _logger = logger;

            _sessions = new List<SessionInfo>();
            _lockObj = new Object();

            SessionFactory = sessionFactory.SessionFactory;
        }


        public ISession OpenSession()
        {
            var currentContext = Thread.CurrentThread.GetHashCode();
            SessionInfo sessionInfo;

            lock (_lockObj)
            {
                sessionInfo = _sessions.FirstOrDefault(x => x.ContextId == currentContext);
            }

            if (sessionInfo == null)
            {
                var session = SessionFactory.OpenSession();
                session.FlushMode = FlushMode.Commit;


                sessionInfo = new SessionInfo
                {
                    ContextId = currentContext,
                    ReferenceCount = 1,
                    Session = session,
                };

                lock (_lockObj)
                {
                    _sessions.Add(sessionInfo);
                }

                //_logger.Trace("Open session [Context: {0}]", currentContext);
            }
            else
            {
                sessionInfo.ReferenceCount++;
                //_logger.Trace("Reuse session [Context: {0}, Reference count: {1}]", currentContext, sessionInfo.ReferenceCount);
            }

            //_logger.Trace("Total sessions: {0}", string.Join("; ", _sessions.Count));

            return sessionInfo.Session;
        }

        public void CloseSession()
        {
            var currentContext = Thread.CurrentThread.GetHashCode();

            SessionInfo sessionInfo;
            lock (_lockObj)
            {
                sessionInfo = _sessions.FirstOrDefault(x => x.ContextId == currentContext);
            }

            if (sessionInfo != null)
            {
                sessionInfo.ReferenceCount--;
                if (sessionInfo.ReferenceCount == 0)
                {
                    lock (_lockObj)
                    {
                        _sessions.Remove(sessionInfo);
                    }

                    sessionInfo.Session.Close();
                    sessionInfo.Session.Dispose();

                    //_logger.Trace("Close session [Context: {0}]", currentContext);
                }
                   // _logger.Trace("Release session [Context: {0}, Reference count: {1}]", currentContext, sessionInfo.ReferenceCount);
            }
            //_logger.Trace("Total sessions: {0}", string.Join("; ", _sessions.Count));
        }

        public SessionScope CreateSessionScope()
        {
            return new SessionScope(this);
        }

        public void OpenSessionForContext(IInterceptor interceptor)
        {
            throw new NotSupportedException("Use OpenSession instead");
        }

        public void OpenSessionForContext()
        {
            throw new NotSupportedException("Use OpenSession instead");
        }

        public void CloseCurrentContextSession()
        {
            throw new NotSupportedException("Use CloseSession instead");
        }
    }
}