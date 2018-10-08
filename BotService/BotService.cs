using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Services;
using Ninject;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace BotService
{
    public partial class BotService : ServiceBase
    {
        static ITelegramBotClient botClient;

        static List<BotUser> _users = new List<BotUser>()
                                      {
                                          new BotUser()
                                          {
                                              Id         = Guid.NewGuid(),
                                              FirstName  = "Gleb",
                                              LastName   = "Agafonov",
                                              TelegramId = 317127863
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
        }

        private static void CreateKernel()
        {
            var kernel = new KernelConfiguration();
            try
            {

                RegisterServices(kernel);
                ServiceLocator.SetRoot(kernel.BuildReadonlyKernel());
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        private static void RegisterServices(IKernelConfiguration kernel)
        {
            kernel.Load(new AppNinjectModule());
        }

        protected override void OnStop()
        {
        }
    }
}