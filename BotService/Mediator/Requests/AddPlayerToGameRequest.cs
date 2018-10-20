using System;
using System.Collections.Generic;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class AddPlayerToGameRequest : IRequest
    {
        public Guid       GameId    { get; set; }
        public List<Guid> PlayerIds { get; set; }
    }
}