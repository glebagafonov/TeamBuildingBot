using System;
using System.IO;
using System.Threading.Tasks;

namespace BotService.Services.Interfaces
{
    public interface ICommunicator
    {
        void SendMessage(string text);
        void SendImage(MemoryStream stream);
    }
}