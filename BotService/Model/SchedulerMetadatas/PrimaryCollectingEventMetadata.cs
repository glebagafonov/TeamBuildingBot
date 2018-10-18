using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public class PrimaryCollectingEventMetadata : IGameScheduledEventMetadata
    {
        public Guid GameId { get; set; }

        public override string ToString()
        {
            return $"PrimaryCollectingEventMetadata: Game:{GameId}";
        }        
    }
}