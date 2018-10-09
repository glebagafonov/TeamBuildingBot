using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Attributes;
using MediatR;

namespace BotService.Requests
{
    [UserAccess(EUserRole.Administrator)]
    public class BindUserToPlayerRequest : IRequest
    {
        public BotUser User               { get; set; }
        public int     SkillValue         { get; set; }
        public int     ParticipationRatio { get; set; }
        public bool    IsActive           { get; set; }
    }
}