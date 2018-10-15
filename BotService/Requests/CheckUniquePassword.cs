using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using MediatR;

namespace BotService.Requests
{
    public class CheckUniquePassword : IRequest<bool>
    {
        public CheckUniquePassword(string password)
        {
            Password = password;
        }
        public string Password  { get; set; }
    }
}