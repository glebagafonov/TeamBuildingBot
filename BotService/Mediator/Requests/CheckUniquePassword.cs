using MediatR;

namespace BotService.Mediator.Requests
{
    public class CheckUniqueLogin : IRequest<bool>
    {
        public CheckUniqueLogin(string login)
        {
            Login = login;
        }
        public string Login  { get; set; }
    }
}