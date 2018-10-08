using System.Threading.Tasks;
using Quartz;

namespace Bot.Infrastructure.Services
{
    public delegate void JobExecutedEventHandler(object sender, IJobExecutionContext context);

    public class QuartzJob : IJob
    {
        public static string  MetadataKey => "MetadataKey";
        public static string EventKey => "EventKey";

        protected event JobExecutedEventHandler JobExecuted;

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                JobExecuted = (context.JobDetail.JobDataMap.Get(EventKey) as JobExecutedEventHandler);
                JobExecuted?.Invoke(this, context); 
            });
        }
    }
}
