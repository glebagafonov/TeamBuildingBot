using System;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Requests
{
    public class SetGameResultRequest : IRequest
    {
        public Guid GameId            { get; set; }
        public int  TeamWinningNumber { get; set; }
        public int  GoalDifference    { get; set; }
    }
}