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
using MediatR;

namespace BotService.Handlers
{
    public class BindUserToPlayerRequestHandler : IRequestHandler<BindUserToPlayerRequest>
    {
        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;

        public BindUserToPlayerRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, 
            IPlayerRepository playerRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository = playerRepository;
        }

        public Task<Unit> Handle(BindUserToPlayerRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                _playerRepository.Save(new Player()
                                       {
                                           Id                 = Guid.NewGuid(),
                                           User               = request.User,
                                           ParticipationRatio = request.ParticipationRatio,
                                           IsActive           = request.IsActive,
                                           SkillValue         = request.SkillValue
                                       });
                _logger.Info($"[{request.User.Id}]: Player added");
            }
            return Unit.Task;
        }
    }
}