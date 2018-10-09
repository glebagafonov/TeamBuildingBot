using Bot.Domain.Entities.Base;

namespace Bot.Domain.Entities
{
    public class VkAccount : BaseAccount
    {
        public virtual int VkId { get; set; }
    }
}