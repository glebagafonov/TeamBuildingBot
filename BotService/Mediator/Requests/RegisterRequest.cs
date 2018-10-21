using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class RegisterRequest : IRequest
    {
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Login     { get; set; }
        public string Password { get; set; }

        public ICommunicator Communicator { get; }

        public RegisterRequest(ICommunicator communicator)
        {
            Communicator = communicator;
        }

        public RegisterRequest()
        {
        }

    }
}