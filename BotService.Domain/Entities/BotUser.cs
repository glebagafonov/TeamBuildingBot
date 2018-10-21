using System.Collections.Generic;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;

namespace Bot.Domain.Entities
{
    public class BotUser : Entity
    {
        public virtual string                   FirstName    { get; set; }
        public virtual string                   LastName     { get; set; }
        public virtual EUserRole                Role         { get; set; }
        public virtual ICollection<BaseAccount> UserAccounts { get; set; }
        public virtual string                   PasswordHash { get; set; }

        public BotUser()
        {
            UserAccounts = new List<BaseAccount>();
        }
    }
}