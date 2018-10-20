using System;
using System.Collections.Generic;

namespace Bot.Domain.Entities
{
    public class TeamPlayer : Entity
    {
        public virtual Player Player     { get; set; }
        public virtual int    TeamNumber { get; set; }
    }
}