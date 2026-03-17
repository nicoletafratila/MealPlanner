using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Queries.GetEdit
{
    /// <summary>
    /// Query to retrieve an application user for editing by username.
    /// </summary>
    public class GetEditQuery : IRequest<ApplicationUserEditModel>
    {
        /// <summary>
        /// The username of the user to edit.
        /// </summary>
        public string? Name { get; set; }

        public GetEditQuery()
        {
        }

        public GetEditQuery(string name)
        {
            Name = name;
        }
    }
}