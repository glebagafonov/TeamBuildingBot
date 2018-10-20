using System;
using System.Runtime.Serialization;

namespace Bot.Infrastructure.Exceptions
{
    public class InvalidDialogActionTypeException : Exception
    {
        public InvalidDialogActionTypeException(string message)
            : base(message)
        {

        }

        public InvalidDialogActionTypeException()
            : base()
        {

        }

        protected InvalidDialogActionTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}