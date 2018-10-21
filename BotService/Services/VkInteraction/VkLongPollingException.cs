using System;

namespace BotService.Services.VkInteraction
{
    public class VkLongPollingException : Exception
    {
        public int Error { get; set; }

        public VkLongPollingException(int error)
        {
            Error = error;
        }
    }
}