using System;
using Bot.Domain.Entities;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class NextGameStatusRequest : IRequest<string>
    {
    }
}