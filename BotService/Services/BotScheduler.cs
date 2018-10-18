using System;
using Bot.Infrastructure.Model;
using Bot.Infrastructure.Services;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using MediatR;

namespace BotService.Services
{
    public class BotScheduler : QuartzBasedScheduler
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public BotScheduler(IMediator mediator, ILogger logger)
        {
            _mediator = mediator;
            _logger = logger;

            ScheduledEventHappened += EventHandler;
        }

        private async void EventHandler(object sender, ScheduleEventHandlerArgs args)
        {
            var eventType = typeof(ScheduledEventRequest<>);
            eventType = eventType.MakeGenericType(args.ScheduledEventMetadata.GetType());
            var @event = (IRequest) Activator.CreateInstance(eventType, args.ScheduledEventMetadata);
            await _mediator.Send(@event);
            _logger.Trace($"ScheduledEventHappened: {args.ScheduledEventMetadata}");
        }
    }
}