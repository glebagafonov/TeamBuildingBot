using NHibernate;

namespace Bot.Infrastructure.Services
{
    public interface IAppSessionFactory
    {
        ISessionFactory SessionFactory { get; }
    }
}