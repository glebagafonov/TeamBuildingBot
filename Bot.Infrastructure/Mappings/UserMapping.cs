using Bot.Domain.Entities;
using Bot.Infrastructure.Mappings.Bases;
using Bot.Infrastructure.Mappings.Helpers;
using NHibernate.Mapping.ByCode;

namespace Bot.Infrastructure.Mappings
{
    public class UserMapping : BaseMapping<BotUser>
    {
        public UserMapping()
        {
            Table("`User`");
            this.PropertyString(x => x.FirstName);
            this.PropertyString(x => x.LastName);
            this.PropertyEnum(x => x.Role);
            this.Property(x => x.TelegramId);
        }
    }
}