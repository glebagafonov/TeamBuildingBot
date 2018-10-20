using ASUDD.Common.Infrastructure.Services;
using NHibernate;

namespace Bot.Infrastructure.Services.Interfaces
{
    public interface IThreadContextSessionProvider : ISessionProvider
    {
        ISession OpenSession();
        void CloseSession();

        SessionScope CreateSessionScope();
    }
}