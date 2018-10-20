using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BotService
{
    static class Program
    {
        internal static void Main(string[] args)
        {
            if (Environment.UserInteractive || args.Contains("--forceConsole"))
            {
                if (args.Contains("--install"))
                {
                    ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                    return;
                }
                if (args.Contains("--uninstall"))
                {
                    ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                    return;
                }
                BotService.StartInConsole(args);
            }
            else
            {
                ServiceBase.Run(new ServiceBase[] { new BotService() });
            }
        }
    }
}
