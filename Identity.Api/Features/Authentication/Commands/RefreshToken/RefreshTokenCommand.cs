using Common.Models;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Command to exchange a refresh token for a new JWT and rotated refresh token.
    /// </summary>
    public class RefreshTokenCommand : IRequest<CommandResponse?>
    {
        public RefreshTokenModel? Model { get; set; }
    }
}
