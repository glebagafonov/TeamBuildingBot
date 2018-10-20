using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Infrastructure.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class LogExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        public LogExceptionBehavior(ILogger logger)
        {
            _logger = logger;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                return next();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }
    }
}