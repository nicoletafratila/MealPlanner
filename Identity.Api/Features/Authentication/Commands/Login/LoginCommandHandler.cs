using Common.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(SignInManager<ApplicationUser> signInManager, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginCommandResponse>
    {
        private readonly ILogger<LoginCommandHandler> _logger = logger;

        public async Task<LoginCommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await signInManager.PasswordSignInAsync(request.Model?.Username!, request.Model?.Password!, true, false);

                if (result.Succeeded)
                {
                    return new LoginCommandResponse { Success = result.Succeeded, Message = string.Empty };
                }
                return new LoginCommandResponse { Success = result.Succeeded, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new LoginCommandResponse { Message = "An error occurred when authenticating the user." };
            }
        }
    }
}


