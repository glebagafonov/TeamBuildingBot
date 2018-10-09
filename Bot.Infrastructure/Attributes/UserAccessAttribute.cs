using System;
using Bot.Domain.Enums;

namespace Bot.Infrastructure.Attributes
{
    public class UserAccessAttribute : Attribute
    {
        public EUserRole UserRole { get; }

        public UserAccessAttribute(EUserRole userRole)
        {
            UserRole = userRole;
        }
    }
}