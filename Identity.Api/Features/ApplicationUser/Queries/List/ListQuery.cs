using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Queries.List
{
    public class ListQuery : IRequest<IList<ApplicationUserListModel>>
    {
    }
}
