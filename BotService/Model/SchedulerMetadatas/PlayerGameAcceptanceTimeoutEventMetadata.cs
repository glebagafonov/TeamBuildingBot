using System;
using Bot.Infrastructure.Model;

namespace BotService.Model.SchedulerMetadatas
{
    public class PlayerGameAcceptanceTimeoutEventMetadata : IGameScheduledEventMetadata
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }

        public override string ToString()
        {
            return $"PlayerGameAcceptanceTimeoutEventMetadata: Game:{GameId}, Player:{PlayerId}";
        }        
    }
}