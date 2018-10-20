using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public class DistributionByTeamsEventMetadata : IGameScheduledEventMetadata
    {
        public Guid GameId { get; set; }

        public override string ToString()
        {
            return $"DistributionByTeamsEventMetadata: Game:{GameId}";
        }        
    }
}