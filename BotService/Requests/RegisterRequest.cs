using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Requests
{
    [UserAccess(EUserRole.User)]
    public class RegisterRequest : IRequest
    {
        public string FirstName { get; set; }
        public string LastName  { get; set; }

        public ICommunicator Communicator { get; set; }
        //public string Password   { get; set; }
    }
}