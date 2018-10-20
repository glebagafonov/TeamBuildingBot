using MediatR;

namespace BotService.Mediator.Requests
{
    public class CheckUniquePassword : IRequest<bool>
    {
        public CheckUniquePassword(string password)
        {
            Password = password;
        }
        public string Password  { get; set; }
    }
}