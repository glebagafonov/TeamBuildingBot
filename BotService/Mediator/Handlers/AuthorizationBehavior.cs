//using System;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Bot.Domain.Enums;
//using Bot.Infrastructure.Attributes;
//using Bot.Infrastructure.Exceptions;
//using Bot.Infrastructure.Services.Interfaces;
//using BotService.Model;
//using MediatR;
//
//namespace BotService.Handlers
//{
//    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//    {
//        private readonly ILogger _logger;
//
//        public AuthorizationBehavior(ILogger logger)
//        {
//            _logger = logger;
//        }
//
//        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
//            RequestHandlerDelegate<TResponse> next)
//        {
//            if (request.GetType().GetCustomAttributes().Any(x => x is UserAccessAttribute) && request.GetType().GetCustomAttributes<UserAccessAttribute>().First().UserRole > EUserRole.User &&
//                (ActionContext.User == null ||
//                ActionContext.User.Role <
//                request.GetType().GetCustomAttributes<UserAccessAttribute>().First().UserRole))
//                throw new UnauthorizedException("User does not have permissions for this command");
//
//            return next();
//        }
//    }
//}