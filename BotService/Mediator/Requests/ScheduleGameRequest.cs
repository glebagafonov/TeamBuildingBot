using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class ScheduleGameRequest : IRequest
    {
        public DateTime DateTime { get; set; }

        public ICommunicator Communicator { get; }

        public ScheduleGameRequest(ICommunicator communicator)
        {
            Communicator = communicator;
        }

        public ScheduleGameRequest()
        {
            
        }
        //public string Password   { get; set; }
    }
}