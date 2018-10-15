using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Requests
{
    public class AuthorizeRequest : IRequest<BotUser>
    {
        public ICommunicator Communicator { get; }

        public AuthorizeRequest(ICommunicator communicator)
        {
            Communicator = communicator;
        }
    }
}