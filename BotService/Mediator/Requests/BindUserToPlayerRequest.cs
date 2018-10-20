using System;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class BindUserToPlayerRequest : IRequest
    {
        public Guid UserId             { get; set; }
        public int  SkillValue         { get; set; }
        public int  ParticipationRatio { get; set; }
        public bool IsActive           { get; set; }
    }
}