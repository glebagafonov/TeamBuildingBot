using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class ChangePlayerStateRequestHandler : IRequestHandler<ChangePlayerStateRequest>
    {
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository             _playerRepository;

        public ChangePlayerStateRequestHandler(
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository             = playerRepository;
        }

        public Task<Unit> Handle(ChangePlayerStateRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var player = _playerRepository.ListBySpecification(new UndeletedEntities<Player>()).First(x => x.User.Id == request.UserId);
                player.IsActive = request.IsActive;
                _playerRepository.Save(player);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}