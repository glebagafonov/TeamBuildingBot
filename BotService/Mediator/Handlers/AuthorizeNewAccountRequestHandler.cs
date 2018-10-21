using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Domain.Entities;
using Bot.Domain.Entities.Base;
using Bot.Domain.Enums;
using Bot.Infrastructure.Repositories.Interfaces;
using Bot.Infrastructure.Services.Interfaces;
using Bot.Infrastructure.Specifications;
using BotService.Mediator.Requests;
using BotService.Services.Interfaces;
using BotService.Services.TelegramServices;
using BotService.Services.VkInteraction;
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class AuthorizeNewAccountRequestHandler : IRequestHandler<AuthorizeNewAccountRequest, bool>
    {
        private readonly ILogger                       _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository            _botUserRepository;

        public AuthorizeNewAccountRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<bool> Handle(AuthorizeNewAccountRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var user = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.PasswordHash) &&
                        BCrypt.Net.BCrypt.Verify(request.Password, x.PasswordHash));
                if (user == null)
                {
                    return Task.FromResult(false);
                }

                if (user.Id == request.CurrentUserId)
                    return Task.FromResult(true);
                var currentUser = _botUserRepository.Get(request.CurrentUserId);
                foreach (var currentUserUserAccount in currentUser.UserAccounts)
                {
                    currentUserUserAccount.User = user;
                    user.UserAccounts.Add(currentUserUserAccount);
                }

                _botUserRepository.Save(user);
                currentUser.UserAccounts.Clear();
                _botUserRepository.Save(currentUser);
                _botUserRepository.Delete(currentUser);
            }
                return Task.FromResult(true);
        }
    }
}