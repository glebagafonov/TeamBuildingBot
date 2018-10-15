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
    public class AuthorizeRequestHandler : IRequestHandler<AuthorizeRequest, BotUser>
    {
        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;

        public AuthorizeRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<BotUser> Handle(AuthorizeRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var user = GetUserByCommunicator(request.Communicator,
                    _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>()));
                
                if (user == null)
                {
                    user = new BotUser()
                           {
                               Id           = Guid.NewGuid(),
                               Role         = EUserRole.UnregisteredUser,
                               UserAccounts = new List<BaseAccount>(),
                              // PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                           };
                    user.UserAccounts.Add(GetAccount(request.Communicator, user));
                    
                    _logger.Info("User added");
                    _botUserRepository.Save(user);
                }
                
                return Task.FromResult(user);
            }
        }

        private BaseAccount GetAccount(ICommunicator communicator, BotUser user)
        {
            switch (communicator)
            {
                    case TelegramCommunicator telegramCommunicator:
                        return new TelegramAccount()
                               {
                                   Id         = Guid.NewGuid(),
                                   TelegramId = telegramCommunicator.TelegramId,
                                   User       = user 
                               };
                    default:
                        throw new InvalidOperationException();
            }
        }

        private BotUser GetUserByCommunicator(ICommunicator communicator, IEnumerable<BotUser> users)
        {
            switch (communicator)
            {
                case TelegramCommunicator telegramCommunicator:
                    return users
                        .FirstOrDefault(x =>
                            x.UserAccounts.Any(
                                y => y is TelegramAccount account && account.TelegramId == telegramCommunicator.TelegramId));
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}