using System;

namespace Bot.Infrastructure.Model
{
    public class ScheduleEventHandlerArgs
    {
        public DateTime Time { get; }
        public IScheduledEventMetadata ScheduledEventMetadata { get; }

        public ScheduleEventHandlerArgs(DateTime time, IScheduledEventMetadata metaData )
        {
            Time = time;
            ScheduledEventMetadata = metaData;
        }
    }
}