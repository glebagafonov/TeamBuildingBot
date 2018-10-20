using System;
using System.Collections.Generic;

namespace Bot.Domain.Entities
{
    public class SortedPlayer : Entity
    {
        public virtual Player Player     { get; set; }
        public virtual int    OrderNumber { get; set; }
    }
}