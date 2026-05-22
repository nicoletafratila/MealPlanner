using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<CommandResponse?>
    {
        public ChangePasswordModel? Model { get; set; }
    }
}
