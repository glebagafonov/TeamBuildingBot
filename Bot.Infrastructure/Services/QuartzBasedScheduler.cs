using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Infrastructure.Model;
using Bot.Infrastructure.Services.Interfaces;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using IScheduler = Bot.Infrastructure.Services.Interfaces.IScheduler;

namespace Bot.Infrastructure.Services
{
    public class QuartzBasedScheduler : IScheduler
    {
        public event ScheduleEventHandler ScheduledEventHappened;
        public string                     ActiveCalendar { get; set; }
        protected StdSchedulerFactory SchedulerFactory;
        protected Quartz.IScheduler Scheduler;
        protected event JobExecutedEventHandler JobExecuted;

        public QuartzBasedScheduler()
        {
            SchedulerFactory = new StdSchedulerFactory();
        }

        private void QuartzBasedScheduler_JobExecuted(object sender, IJobExecutionContext context)
        {
            if (context.Trigger.CalendarName == ActiveCalendar)
            {
                var metadata = context.JobDetail.JobDataMap[QuartzJob.MetadataKey] as IScheduledEventMetadata;
                ScheduledEventHappened?.Invoke(sender, new ScheduleEventHandlerArgs(DateTime.Now, metadata));
            }
        }

        public async void AddEvent(string calendarName, IScheduledEventMetadata data, DateTime time)
        {
            var now      = time.ToLocalTime();
            var cronexpr = $"{now.Second} {now.Minute} {now.Hour} {now.Day} {now.Month} ? {now.Year}";
            var map = new JobDataMap
                      {
                          {QuartzJob.MetadataKey, data},
                          {QuartzJob.EventKey, JobExecuted}
                      };

            var job = JobBuilder.Create<QuartzJob>()
                .WithIdentity($"{calendarName} {data} {now}", calendarName)
                .SetJobData(map)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{calendarName} {data} {now}", calendarName)
                .StartNow()
                .WithCronSchedule(cronexpr)
                .ModifiedByCalendar(calendarName)
                .Build();

            if (!(await Scheduler.CheckExists(job.Key)))
                await Scheduler.ScheduleJob(job, trigger);
        }

        public async void DeleteEventByDataType<T>()
            where T : IScheduledEventMetadata
        {
            var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var jobKey in jobKeys)
            {
                if (jobKey.Name.Contains(typeof(T).Name))
                {
                    await Scheduler.DeleteJob(jobKey);
                }
            }
        }

        public async Task<bool> DoesContainEventByDataType<T>() where T : IScheduledEventMetadata
        {
            var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var jobKey in jobKeys)
            {
                if (jobKey.Name.Contains(typeof(T).Name))
                {
                    return true;
                }
            }

            return false;
        }

        public async void Start()
        {
            Scheduler   =  await SchedulerFactory.GetScheduler();
            JobExecuted += QuartzBasedScheduler_JobExecuted;
            if (Scheduler.InStandbyMode)
            {
                await Scheduler.Start();
            }
        }

        public async void Stop()
        {
            var triggerKeys = await Scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in triggerKeys)
            {
                var trigger = await Scheduler.GetTrigger(triggerKey);
                await Scheduler.DeleteJob(trigger.JobKey);
            }

            await Scheduler.Standby();
        }

        public async void ClearScheduledEvents()
        {
            var jobKeys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var jobKey in jobKeys)
            {
                await Scheduler.DeleteJob(jobKey);
            }
        }
    }
}