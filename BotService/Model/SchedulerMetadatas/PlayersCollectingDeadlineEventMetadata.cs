using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public class PlayersCollectingDeadlineEventMetadata : IGameScheduledEventMetadata
    {
        public Guid GameId { get; set; }

        public override string ToString()
        {
            return $"PlayersCollectingDeadlineEventMetadata: Game:{GameId}";
        }        
    }
}