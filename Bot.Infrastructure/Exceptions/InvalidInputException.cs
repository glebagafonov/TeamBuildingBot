using System;
using System.Runtime.Serialization;

namespace Bot.Infrastructure.Exceptions
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message)
            : base(message)
        {

        }

        public InvalidInputException()
            : base()
        {

        }

        protected InvalidInputException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}