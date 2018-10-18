using Bot.Infrastructure.Model;

namespace BotService.Mediator.Requests.ScheduledEventRequests.Base
{
    public class ScheduledEventRequest<TMetaDataType> : IScheduledEventRequest<TMetaDataType>
        where TMetaDataType : IScheduledEventMetadata
    {
        public TMetaDataType MetaData { get; }

        public ScheduledEventRequest(TMetaDataType metaData)
        {
            MetaData = metaData;
        }
    }
}