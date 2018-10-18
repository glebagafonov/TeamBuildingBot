using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public class PlayersCollectedEventMetadata : IScheduledEventMetadata
    {
        public Guid GameId { get; set; }

        public override string ToString()
        {
            return $"PlayersCollectedEventMetadata: Game:{GameId}";
        }        
    }
}