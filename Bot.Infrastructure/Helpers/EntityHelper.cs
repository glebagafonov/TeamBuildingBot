using System;
using Bot.Domain.Entities;

namespace Bot.Infrastructure.Helpers
{
    public static class EntityHelper
    {
        public static void MarkAsNew(this Entity entity)
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();
           // entity.State = EEntityState.Normal;
           // entity.Timestamp = DateTime.UtcNow;
        }

//        public static void MarkAsDeleted(this Entity entity)
//        {
//           // entity.State = EEntityState.Deleted;
//            entity.Timestamp = DateTime.UtcNow;
//        }
//
//        public static void MarkAsModified(this Entity entity)
//        {
//            //entity.Timestamp = DateTime.UtcNow;
//        }
    }
}
