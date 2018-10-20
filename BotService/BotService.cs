using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Model;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using MediatR;
using Ninject;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BotService
{
    public partial class BotService : ServiceBase
    {
        static List<BotUser> _users = new List<BotUser>()
                                      {
                                          new BotUser()
                                          {
                                              Id         = Guid.NewGuid(),
                                              FirstName  = "Gleb",
                                              LastName   = "Agafonov",
                                              UserAccounts = new List<BaseAccount>()
                                                             {
                                                                 new VkAccount()
                                                                 {
                                                                     Id = Guid.NewGuid(), VkId = 123
                                                                 },
                                                                 
                                                                 new TelegramAccount()
                                                                 {
                                                                     Id = Guid.NewGuid(),
                                                                     TelegramId = 317127863
                                                                 }
                                                             },
                                              Role = EUserRole.Administrator
                                          }
                                      };

        public BotService()
        {
            CreateKernel();
            InitializeComponent();
        }

        public static void StartInConsole(string[] args)
        {
            var service = new BotService();
            service.OnStart(args);
            Console.WriteLine(@"Press any key to stop program");
            Console.Read();
            service.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            ServiceLocator.Get<TelegramInteractionService>();
            ServiceLocator.Get<IScheduler>();
        }

        private static void CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {

                RegisterServices(kernel);
                ServiceLocator.SetRoot(kernel);
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private static void RegisterServices(StandardKernel kernel)
        {
            kernel.Load(new AppNinjectModule());
        }

        protected override void OnStop()
        {
        }
    }
}