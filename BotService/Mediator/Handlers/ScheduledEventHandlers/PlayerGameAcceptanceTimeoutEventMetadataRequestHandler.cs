using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Mediator.Requests.ScheduledEventRequests;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using MediatR;

namespace BotService.Mediator.Handlers.ScheduledEventHandlers
{
    public class PlayerGameAcceptanceTimeoutEventMetadataRequestHandler : IRequestHandler<
        ScheduledEventRequest<PlayerGameAcceptanceTimeoutEventMetadata>>
    {
        private readonly IMediator _mediator;

        public PlayerGameAcceptanceTimeoutEventMetadataRequestHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<Unit> Handle(ScheduledEventRequest<PlayerGameAcceptanceTimeoutEventMetadata> message,
            CancellationToken cancellationToken)
        {
            _mediator.Send(new PlayerGameDecisionRequest()
                           {GameId = message.MetaData.GameId, PlayerId = message.MetaData.PlayerId, Decision = false});

            return Task.FromResult(Unit.Value);
        }
    }
}