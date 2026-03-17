using AutoMapper;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving an application user for editing by username.
    /// </summary>
    public class GetEditQueryHandler(
        UserManager<Common.Data.Entities.ApplicationUser> userManager,
        IMapper mapper) : IRequestHandler<GetEditQuery, ApplicationUserEditModel>
    {
        private readonly UserManager<Common.Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<ApplicationUserEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new ApplicationUserEditModel();
            }

            var user = await _userManager.FindByNameAsync(request.Name);
            if (user is null)
            {
                return new ApplicationUserEditModel();
            }

            var model = _mapper.Map<ApplicationUserEditModel>(user);
            return model ?? new ApplicationUserEditModel();
        }
    }
}