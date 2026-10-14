using Common.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    public class ResendConfirmationEmailCommand : IRequest<CommandResponse?>
    {
        public string? UserId { get; set; }
    }
}
