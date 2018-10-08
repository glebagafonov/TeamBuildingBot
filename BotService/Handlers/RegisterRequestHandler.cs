using System.Threading;
using System.Threading.Tasks;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Requests;
using MediatR;

namespace BotService.Handlers
{
    public class RegisterRequestHandler : IRequestHandler<RegisterRequest>
    {
        private readonly ILogger _logger;

        public RegisterRequestHandler(ILogger logger)
        {
            _logger = logger;
        }
        
        public Task<Unit> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            _logger.Info("Wow it's ok");
            return Unit.Task;
        }
    }
}