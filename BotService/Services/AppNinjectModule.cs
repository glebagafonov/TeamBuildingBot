using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories;
using Bot.Infrastructure.Repositories.Base;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Handlers;
using BotService.Handlers.DummyHandlers;
using BotService.Mediator.Handlers;
using BotService.Mediator.Handlers.ScheduledEventHandlers;
using BotService.Mediator.Requests;
using BotService.Mediator.Requests.ScheduledEventRequests;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Ninject;
using BotService.Providers;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using BotService.Services.VkInteraction;
using MediatR;
using MediatR.Pipeline;
using NHibernate;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Syntax;

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
            Kernel?.Components.Add<IBindingResolver, ContravariantBindingResolver>();

            Kernel?.Bind(scan => scan.FromAssemblyContaining<IMediator>().SelectAllClasses().BindDefaultInterface());
            Kernel?.Bind(scan => scan.FromAssemblyContaining<PrimaryCollectingEventMetadataRequestHandler>()
                .SelectAllClasses()
                .InheritedFrom(typeof(IRequestHandler<,>)).BindAllInterfaces());
            Kernel?.Bind(scan =>
                scan.FromAssemblyContaining<PrimaryCollectingEventMetadataRequestHandler>().SelectAllClasses());

            //Pipeline
            Kernel?.Bind(typeof(IPipelineBehavior<,>)).To(typeof(RequestPreProcessorBehavior<,>));
            Kernel?.Bind(typeof(IPipelineBehavior<,>)).To(typeof(LogExceptionBehavior<,>));
            //Kernel?.Bind(typeof(IPipelineBehavior<,>)).To(typeof(AuthorizationBehavior<,>));

            Bind<ServiceFactory>().ToMethod(ctx => t => ctx.Kernel.TryGet(t));
        }

        private void BindServices()
        {
            Bind<IScheduler>()
                .To<BotScheduler>()
                .InSingletonScope()
                .OnActivation(x => x.Start());
            
            Bind<ICommunicatorFactory>()
                .To<CommunicatorFactory>()
                .InSingletonScope();

            Bind<VkInteractionService>()
                .ToSelf()
                .InSingletonScope();

            Bind<TelegramInteractionService>()
                .ToSelf()
                .InSingletonScope();

            Bind<CommandFactory>()
                .ToSelf()
                .InSingletonScope()
                .OnActivation(x => x.Initialize());

            Bind<IUserInteractionService>()
                .To<UserInteractionService>()
                .InSingletonScope();

            Bind<IDialogStorage>()
                .To<DialogStorage>()
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


    public static class BindingExtensions
    {
        public static IBindingInNamedWithOrOnSyntax<object> WhenNotificationMatchesType<TNotification>(
            this IBindingWhenSyntax<object> syntax)
            where TNotification : INotification
        {
            return syntax.When(request =>
                typeof(TNotification).IsAssignableFrom(request.Service.GenericTypeArguments.Single()));
        }
    }
}