using Common.Services;
using Identity.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Features.ApplicationUser.Queries.List
{
    /// <summary>
    /// Handles retrieving a lightweight list of other active users, for pickers such as recipe cloning.
    /// </summary>
    public class ListQueryHandler(
        UserManager<Data.Entities.ApplicationUser> userManager,
        ICurrentUserService currentUserService) : IRequestHandler<ListQuery, IList<ApplicationUserListModel>>
    {
        private readonly UserManager<Data.Entities.ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<IList<ApplicationUserListModel>> Handle(ListQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId;

            return await _userManager.Users
                .Where(u => u.IsActive && u.Id != currentUserId)
                .OrderBy(u => u.UserName)
                .Select(u => new ApplicationUserListModel
                {
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync(cancellationToken);
        }
    }
}
