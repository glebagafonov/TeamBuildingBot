using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Enums;
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
    public class CancelGameRequestHandler : IRequestHandler<CancelGameRequest>
    {

        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository            _botUserRepository;
        private readonly IScheduler                    _scheduler;
        private readonly IGameRepository               _gameRepository;
        private readonly IUserInteractionService _userInteractionService;

        public CancelGameRequestHandler(
            IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository,
            IScheduler scheduler,
            IGameRepository gameRepository,
            IUserInteractionService userInteractionService)
        {
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _scheduler                    = scheduler;
            _gameRepository               = gameRepository;
            _userInteractionService = userInteractionService;
        }

        public Task<Unit> Handle(CancelGameRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var game = _gameRepository.Get(request.GameId);
                var administrators = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .Where(x => x.Role >= EUserRole.Moderator).ToList();
                NotifyAboutUnsuccessfulCollecting(game, administrators);
                NotifyPlayers(game);
                _scheduler.DeleteEvent<IGameScheduledEventMetadata>(x => x.GameId == game.Id);
                game.IsActive = false;
                _gameRepository.Save(game);
            }
            return Task.FromResult(Unit.Value);
        }
        
        private void NotifyAboutUnsuccessfulCollecting(Game game, List<BotUser> administrators)
        {
            var message =
                $"Игра {game.DateTime.ToString(DateTimeHelper.DateFormat)} отменена";

            administrators.ForEach(x => _userInteractionService.SendMessage(message, x));
        }

        private void NotifyPlayers(Game game)
        {
            var usersForNotify = game.AcceptedPlayers.Select(x => x.User)
                .Concat(game.RequestedPlayers.Select(x => x.Player.User));
            
            var message =
                $"Игра {game.DateTime.ToString(DateTimeHelper.DateFormat)} отменена";
            
            foreach (var user in usersForNotify)
            {
                _userInteractionService.StopDialog(user);
                _userInteractionService.SendMessage(message, user);
            }
        }
    }
}