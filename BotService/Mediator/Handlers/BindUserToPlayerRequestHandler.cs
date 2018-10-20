using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class BindUserToPlayerRequestHandler : IRequestHandler<BindUserToPlayerRequest>
    {
        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;
        private readonly IBotUserRepository _botUserRepository;

        public BindUserToPlayerRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, 
            IPlayerRepository playerRepository,
            IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository = playerRepository;
            _botUserRepository = botUserRepository;
        }

        public Task<Unit> Handle(BindUserToPlayerRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var user = _botUserRepository.Get(request.UserId);
                user.Role = EUserRole.Player;
                _botUserRepository.Save(user);
                _playerRepository.Save(new Player()
                                       {
                                           Id                 = Guid.NewGuid(),
                                           User               = user,
                                           ParticipationRatio = request.ParticipationRatio,
                                           IsActive           = request.IsActive,
                                           SkillValue         = request.SkillValue
                                       });
                _logger.Info($"[{user.Id}]: Player added");
            }
            return Unit.Task;
        }
    }
}