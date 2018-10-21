using System;
using Bot.Domain.Entities;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class AuthorizeNewAccountRequest : IRequest<bool>
    {
        public Guid   CurrentUserId { get; set; }
        public string Login         { get; set; }
        public string Password      { get; set; }
    }
}