using System;
using System.IO;
using BotService.Services.Interfaces;

namespace BotService.Services.VkInteraction
{
    public class VkCommunicator : ICommunicator
    {
        public  long                 VkId                 { get; }
        private VkInteractionService VkInteractionService { get; }

        public VkCommunicator(long vkId, VkInteractionService telegramInteractionService)
        {
            VkInteractionService = telegramInteractionService;
            VkId                 = vkId;
        }

        public void SendMessage(string text)
        {
            VkInteractionService.SendMessage(VkId, text);
        }

        public void SendImage(MemoryStream stream)
        {
            throw new NotImplementedException();
            //VkInteractionService.SendImage(TelegramId, stream).Wait();
        }
    }
}