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
    public class RegisterRequestByTelegramAccountHandler : IRequestHandler<RegisterRequestByTelegramAccount>
    {
        private readonly ILogger _logger;
        private readonly IThreadContextSessionProvider _threadContextSessionProvider;
        private readonly IBotUserRepository _botUserRepository;

        public RegisterRequestByTelegramAccountHandler(ILogger logger,
            IThreadContextSessionProvider threadContextSessionProvider, IBotUserRepository botUserRepository)
        {
            _logger                       = logger;
            _threadContextSessionProvider = threadContextSessionProvider;
            _botUserRepository            = botUserRepository;
        }

        public Task<Unit> Handle(RegisterRequestByTelegramAccount request, CancellationToken cancellationToken)
        {
            using (_threadContextSessionProvider.CreateSessionScope())
            {
                var user = _botUserRepository.ListBySpecification(new UndeletedEntities<BotUser>())
                    .FirstOrDefault(x =>
                        x.UserAccounts.Any(
                            y => y is TelegramAccount account && account.TelegramId == request.TelegramId));
                if (user == null)
                {
                    user = new BotUser()
                           {
                               Id           = Guid.NewGuid(),
                               FirstName    = request.FirstName,
                               LastName     = request.LastName,
                               Role         = EUserRole.User,
                               UserAccounts = new List<BaseAccount>()
                           };
                    user.UserAccounts.Add(new TelegramAccount()
                                          {
                                              Id         = Guid.NewGuid(),
                                              TelegramId = request.TelegramId,
                                              User       = user
                                          });
                    
                    _logger.Info("User added");
                }
                else
                {
                    user.FirstName = request.FirstName;
                    user.LastName = request.LastName;
                    _logger.Info("User updated");
                }

                _botUserRepository.Save(user);
            }
            return Unit.Task;
        }
    }
}