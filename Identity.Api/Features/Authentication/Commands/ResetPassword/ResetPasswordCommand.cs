using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<CommandResponse?>
    {
        public ResetPasswordModel? Model { get; set; }
    }
}
