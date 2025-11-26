using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        public ApplicationUserEditModel? Model { get; set; }
    }
}
