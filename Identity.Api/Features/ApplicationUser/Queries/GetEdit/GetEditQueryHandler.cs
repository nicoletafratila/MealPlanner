using AutoMapper;
using Common.Data.Entities;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    public class GetEditQueryHandler(UserManager<Common.Data.Entities.ApplicationUser> userManager, IMapper mapper) : IRequestHandler<GetEditQuery, ApplicationUserEditModel>
    {
        public async Task<ApplicationUserEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await userManager.FindByNameAsync(request.Name!);
            return mapper.Map<ApplicationUserEditModel>(result);
        }
    }
}
