using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class ChangePlayerStateRequest : IRequest
    {
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
    }
}