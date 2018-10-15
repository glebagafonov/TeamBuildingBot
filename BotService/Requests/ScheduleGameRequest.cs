using System;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Requests
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