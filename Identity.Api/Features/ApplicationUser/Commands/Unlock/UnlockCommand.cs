using Common.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Commands.Unlock
{
    public class UnlockCommand : IRequest<CommandResponse?>
    {
        public string? UserId { get; set; }
    }
}
