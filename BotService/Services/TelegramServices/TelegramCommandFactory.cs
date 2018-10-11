using System;
using System.Collections.Generic;
using BotService.Requests;
using MediatR;

namespace BotService.Services.TelegramServices
{
    
    public class TelegramCommandFactory
    {
        private readonly Dictionary<string, Type> _requests;
        public TelegramCommandFactory()
        {
            _requests = new Dictionary<string, Type>();
            
        }

        public void Initialize()
        {
            Add<RegisterRequest>("/register");
        }

        private void Add<TRequest>(string command)
            where TRequest : IRequest
        {
            _requests.Add(command, typeof(TRequest));
        }
        
        private void Add<TRequest, TResponse>(string command)
            where TRequest : IRequest<TResponse>
        {
            _requests.Add(command, typeof(TRequest));
        }
    }
}