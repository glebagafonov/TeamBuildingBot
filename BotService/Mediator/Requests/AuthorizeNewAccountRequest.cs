using System;
using Bot.Domain.Entities;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class AuthorizeNewAccountRequest : IRequest<bool>
    {
        public ICommunicator Communicator  { get; }
        public Guid          CurrentUserId { get; set; }
        public string        Password      { get; set; }

        public AuthorizeNewAccountRequest(ICommunicator communicator)
        {
            Communicator = communicator;
        }
    }
}