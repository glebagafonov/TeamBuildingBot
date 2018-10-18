using Bot.Domain.Entities;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
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