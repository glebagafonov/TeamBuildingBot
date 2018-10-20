using System;
using System.Runtime.Serialization;

namespace Bot.Infrastructure.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message)
            : base(message)
        {

        }

        public UnauthorizedException()
            : base()
        {

        }

        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}