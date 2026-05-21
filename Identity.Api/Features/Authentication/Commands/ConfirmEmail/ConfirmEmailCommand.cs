using Common.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.ConfirmEmail
{
    public class ConfirmEmailCommand : IRequest<CommandResponse?>
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
