using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Requests;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using MediatR;

namespace BotService.Handlers
{
    public class ScheduleGameRequestHandler : IRequestHandler<ScheduleGameRequest>
    {
        private const int MaxUsersCount = 500;

        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;

        public ScheduleGameRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<Unit> Handle(ScheduleGameRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                _logger.Trace($"Game date - {request.DateTime}");
                return Unit.Task;
            }
        }
    }
}