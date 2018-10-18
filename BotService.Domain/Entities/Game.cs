using System;
using System.Collections.Generic;

namespace Bot.Domain.Entities
{
    public class Game : Entity
    {
        public virtual DateTime DateTime { get; set; }
        public virtual ICollection<SortedPlayer> SortedPlayersByRating { get; set; }
        public virtual ICollection<PlayerEvent> RequestedPlayers { get; set; }
        public virtual ICollection<Player> AcceptedPlayers { get; set; }
        public virtual ICollection<Player> RejectedPlayers { get; set; }
        public virtual ICollection<TeamPlayer> DistributedPlayers { get; set; }


        public Game()
        {
            SortedPlayersByRating = new List<SortedPlayer>();
            RequestedPlayers = new List<PlayerEvent>();
            RejectedPlayers = new List<Player>();
            AcceptedPlayers = new List<Player>();
            DistributedPlayers = new List<TeamPlayer>();
        }
    }
}