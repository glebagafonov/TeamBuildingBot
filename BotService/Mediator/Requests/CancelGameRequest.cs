using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class CancelGameRequest : IRequest
    {
        public Guid GameId   { get; set; }
    }
}