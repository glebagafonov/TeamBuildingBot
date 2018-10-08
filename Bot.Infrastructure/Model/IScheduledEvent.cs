using System;

namespace Bot.Infrastructure.Model
{
    public interface IScheduledEvent
    {
        DateTime                Time { get; }
        IScheduledEventMetadata Data { get; }
    }
}