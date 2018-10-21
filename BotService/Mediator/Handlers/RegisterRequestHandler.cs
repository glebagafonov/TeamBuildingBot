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
using MediatR;

namespace BotService.Mediator.Handlers
{
    public class RegisterRequestHandler : IRequestHandler<RegisterRequest>
    {
        private const int MaxUsersCount = 500;
        
        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;
        
        public RegisterRequestHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, 
            IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<Unit> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var users = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>()).ToList();
                if(users.Count() > MaxUsersCount)
                    throw new ArgumentOutOfRangeException("—оздано максимальное количество пользователей");
                
                var user = GetUserByCommunicator(request.Communicator, users);
                
                if (user == null)
                {
                    user = new BotUser()
                           {
                               Id           = Guid.NewGuid(),
                               FirstName    = request.FirstName,
                               LastName     = request.LastName,
                               Role         = EUserRole.User,
                               UserAccounts = new List<BaseAccount>(),
                               PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                           };
                    user.UserAccounts.Add(GetAccount(request.Communicator, user));
                    
                    _logger.Info("User added");
                }
                else
                {
                    user.FirstName = request.FirstName;
                    user.LastName = request.LastName;
                    if (user.Role == EUserRole.UnregisteredUser)
                        user.Role = EUserRole.User;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    _logger.Info("User updated");
                }

                _botUserRepository.Save(user);
            }
            return Unit.Task;
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