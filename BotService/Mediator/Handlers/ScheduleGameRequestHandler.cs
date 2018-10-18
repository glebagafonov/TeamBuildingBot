using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using BotService.Mediator.Requests;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class ScheduleGameRequestHandler : IRequestHandler<ScheduleGameRequest>
    {
        private const int MaxUsersCount = 500;

        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;
        private readonly IScheduler _scheduler;
        private readonly IGameRepository _gameRepository;
        private readonly IServiceConfiguration _configuration;

        public ScheduleGameRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider,
            IBotUserRepository botUserRepository,
            IScheduler scheduler,
            IGameRepository gameRepository,
            IServiceConfiguration configuration)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
            _scheduler                    = scheduler;
            _gameRepository               = gameRepository;
            _configuration                = configuration;
        }

        public Task<Unit> Handle(ScheduleGameRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                _logger.Trace($"Schedule game at {request.DateTime}");
                var game = new Game()
                           {
                               DateTime = request.DateTime
                           };
                game.MarkAsNew();
                _gameRepository.Save(game);
                _scheduler.AddEvent(new PrimaryCollectingEventMetadata() { GameId = game.Id},
                    request.DateTime.Subtract(_configuration.GameScheduleThreshold, _configuration.StartDayTime,
                        _configuration.EndDayTime));


                _logger.Trace($"Game date - {request.DateTime}");
                return Unit.Task;
            }
        }
    }
}