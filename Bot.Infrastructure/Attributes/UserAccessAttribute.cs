using System;
using Bot.Domain.Enums;

namespace Bot.Infrastructure.Attributes
{
    public class UserAccessAttribute : Attribute
    {
        public EUserRole UserRole { get; set; }

        public UserAccessAttribute(EUserRole userRole)
        {
            UserRole = userRole;
        }
    }
}