using System;
using System.Collections.Generic;

namespace Bot.Domain.Entities
{
    public class Game : Entity
    {
        public virtual ICollection<Player> Players  { get; set; }
        public virtual DateTime            DateTime { get; set; }
    }
}