using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Exceptions;
using Bot.Infrastructure.Helpers;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Mediator.Requests.ScheduledEventRequests;
using BotService.Mediator.Requests.ScheduledEventRequests.Base;
using BotService.Model.SchedulerMetadatas;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using MediatR;

namespace BotService.Mediator.Handlers.ScheduledEventHandlers
{
    public class PlayerGameAcceptanceTimeoutEventMetadataRequestHandler : IRequestHandler<
        ScheduledEventRequest<PlayerGameAcceptanceTimeoutEventMetadata>>
    {
        private readonly IMediator _mediator;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ICommunicatorFactory _communicatorFactory;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IPlayerRepository _playerRepository;

        public PlayerGameAcceptanceTimeoutEventMetadataRequestHandler(IMediator mediator,
            IUserInteractionService userInteractionService,
            ICommunicatorFactory communicatorFactory,
            IThreadContextSessionProvider threadContextSessionProvider,
            IPlayerRepository playerRepository)
        {
            _mediator = mediator;
            _userInteractionService = userInteractionService;
            _communicatorFactory = communicatorFactory;
            _threadContextSessionProvider = threadContextSessionProvider;
            _playerRepository = playerRepository;
        }

        public Task<Unit> Handle(ScheduledEventRequest<PlayerGameAcceptanceTimeoutEventMetadata> message,
            CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var player = _playerRepository.Get(message.MetaData.PlayerId);
                _userInteractionService.StopDialog(player.User);
                _userInteractionService.SendMessage("Ты не успел принять игру", player.User);
            }
            _mediator.Send(new PlayerGameDecisionRequest()
                           {GameId = message.MetaData.GameId, PlayerId = message.MetaData.PlayerId, Decision = false});

            return Task.FromResult(Unit.Value);
        }
    }
}