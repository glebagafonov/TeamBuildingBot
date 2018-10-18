using System;
using System.Collections.Generic;

namespace Bot.Domain.Entities
{
    public class PlayerEvent : Entity
    {
        public virtual Player   Player    { get; set; }
        public virtual DateTime EventTime { get; set; }
    }
}