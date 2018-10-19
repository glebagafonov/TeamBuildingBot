using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class PlayerStatusRequest : IRequest<bool>
    {
        public Guid UserId   { get; set; }
    }
}