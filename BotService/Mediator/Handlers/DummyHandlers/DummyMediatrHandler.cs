using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace BotService.Handlers.DummyHandlers
{
    public class DummyMediatrHandler : IRequestHandler<IRequest>
    {
        public Task Handle(IRequest message)
        {
            // this handler contains no actions
            // 'cause it's a stub
            return Task.CompletedTask;
        }

        public Task<Unit> Handle(IRequest request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }
    }
}
