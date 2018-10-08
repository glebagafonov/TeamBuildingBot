using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories;
using Bot.Infrastructure.Repositories.Base;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Providers;
using BotService.Services.Interfaces;
using MediatR;
using NHibernate;
using Ninject;
using Ninject.Modules;

namespace BotService.Services
{
    public class AppNinjectModule : NinjectModule
    {
        public override void Load()
        {
            BindServices();
            BindBus();
            BindDbServices();
            BindRepositories();
        }
        
        private void BindDbServices()
        {
            Bind<IThreadContextSessionProvider>()
                .To<BotServiceSessionProvider>()
                .InSingletonScope();

            Bind<ISessionProvider>()
                .ToMethod(context => context.Kernel.Get<IThreadContextSessionProvider>())
                .InSingletonScope();

            Bind<BotServiceSessionFactory>()
                .ToSelf()
                .InSingletonScope();

            Bind<ISessionFactory>()
                .ToMethod(context => context.Kernel.Get<BotServiceSessionFactory>().SessionFactory)
                .InSingletonScope();
        }
        
        private void BindRepositories()
        {
            #region indirect
            
            Bind<IRepository<BotUser>>()
                .ToMethod(context => context.Kernel.Get<IBotUserRepository>())
                .InSingletonScope();
            
            Bind<IRepository<Player>>()
                .ToMethod(context => context.Kernel.Get<IPlayerRepository>())
                .InSingletonScope();
            
            Bind<IRepository<Game>>()
                .ToMethod(context => context.Kernel.Get<IGameRepository>())
                .InSingletonScope();

            #endregion

            #region direct

            Bind<IBotUserRepository>()
                .To<BotUserRepository>()
                .InSingletonScope();

            Bind<IGameRepository>()
                .To<GameRepository>()
                .InSingletonScope();

            Bind<IPlayerRepository>()
                .To<PlayerRepository>()
                .InSingletonScope();
            
            #endregion
        }

        private void BindBus()
        {
            Bind<IMediator>()
                .To<Mediator>()
                .InSingletonScope();
        }
        
        private void BindServices()
        {
            Bind<TelegramInteractionService>()
                .ToSelf()
                .InSingletonScope();
            
            Bind<ILogger>()
                .To<NLogger>()
                .InSingletonScope()
                .WithConstructorArgument("logConfiguration", "fullLog");

            Bind<IServiceConfiguration>()
                .To<ServiceConfiguration>()
                .InSingletonScope();
        }
    }
}