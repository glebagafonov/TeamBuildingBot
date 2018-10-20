using Bot.Infrastructure.Model;
using MediatR;

namespace BotService.Mediator.Requests.ScheduledEventRequests.Base
{
    public interface IScheduledEventRequest<out TMetaDataType> : IRequest
        where TMetaDataType : IScheduledEventMetadata
    {
        TMetaDataType MetaData { get; }
    }
}