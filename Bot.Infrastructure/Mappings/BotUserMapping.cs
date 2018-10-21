using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class BotUserMapping : BaseMapping<BotUser>
    {
        public BotUserMapping()
        {
            Table("`User`");
            this.PropertyString(x => x.FirstName);
            this.PropertyString(x => x.LastName);
            this.PropertyEnum(x => x.Role);
            
            Set(x => x.UserAccounts, c =>
            {
                c.Cascade(Cascade.All);
                c.Key(k => k.Column("UserId"));
                c.Table("User_Account");
            }, r => r.ManyToMany(m => m.Column("AccountId")));
            
            
            this.PropertyString(x => x.PasswordHash);
        }
    }
}