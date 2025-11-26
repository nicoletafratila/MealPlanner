using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    public class GetEditQuery : IRequest<ApplicationUserEditModel>
    {
        public string? Name { get; set; }
    }
}
