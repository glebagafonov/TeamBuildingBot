using NHibernate;

namespace Bot.Infrastructure.Services.Interfaces
{
    public interface ISessionProvider
    {
        ISession Session { get; }
        ISessionFactory SessionFactory { get; }

        void OpenSessionForContext();
        void OpenSessionForContext(IInterceptor interceptor);
        void CloseCurrentContextSession();
    }
}
