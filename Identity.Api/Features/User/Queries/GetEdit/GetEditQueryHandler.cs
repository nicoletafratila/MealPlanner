using AutoMapper;
using Common.Data.Entities;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.User.Queries.GetEdit
{
    public class GetEditQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper) : IRequestHandler<GetEditQuery, ApplicationUserEditModel>
    {
        public async Task<ApplicationUserEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await userManager.FindByNameAsync(request.Name);
            return mapper.Map<ApplicationUserEditModel>(result);
        }
    }
}
