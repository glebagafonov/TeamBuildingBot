using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public interface IGameScheduledEventMetadata : IScheduledEventMetadata
    {
        Guid GameId { get; set; }
    }
}