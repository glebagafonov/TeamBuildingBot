using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class PlayerGameDecisionRequest : IRequest
    {
        public Guid GameId   { get; set; }
        public Guid PlayerId { get; set; }
        public bool Decision { get; set; }
    }
}