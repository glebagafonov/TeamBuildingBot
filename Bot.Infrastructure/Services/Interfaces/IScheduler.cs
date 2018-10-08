using System;
using System.Threading.Tasks;
using Bot.Infrastructure.Model;

namespace Bot.Infrastructure.Services.Interfaces
{
    public interface IScheduler
    {
            void AddEvent(string calendarName, IScheduledEventMetadata data, DateTime time);
            event ScheduleEventHandler ScheduledEventHappened;
            void Start();
            void Stop();
            void ClearScheduledEvents();

            void DeleteEventByDataType<T>()
                where T : IScheduledEventMetadata;
        
            Task<bool> DoesContainEventByDataType<T>()
                where T : IScheduledEventMetadata;
        }

        public delegate void ScheduleEventHandler(object sender, ScheduleEventHandlerArgs args);
}