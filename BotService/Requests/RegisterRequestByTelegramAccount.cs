using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using MediatR;

namespace BotService.Requests
{
    [UserAccess(EUserRole.User)]
    public class RegisterRequestByTelegramAccount : IRequest
    {
        public string FirstName  { get; set; }
        public string LastName   { get; set; }
        public long   TelegramId { get; set; }
    }
}