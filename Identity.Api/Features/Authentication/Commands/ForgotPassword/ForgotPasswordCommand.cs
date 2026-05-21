using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<CommandResponse?>
    {
        public ForgotPasswordModel? Model { get; set; }
    }
}
