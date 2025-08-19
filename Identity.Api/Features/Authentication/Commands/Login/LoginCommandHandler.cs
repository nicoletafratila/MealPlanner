using Common.Data.Entities;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var signInResult = await signInManager.PasswordSignInAsync(request!.Model!.Username!, request!.Model!.Password!, isPersistent: false, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    var user = await userManager.FindByNameAsync(request!.Model!.Username!);
                    return new CommandResponse
                    {
                        Message = "Login successful.",
                        Succeeded = true
                    };
                }

                return new CommandResponse
                {
                    Message = "User/password not found.",
                    Succeeded = false
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return new CommandResponse
                {
                    Message = "An error occurred when authenticating the user.",
                    Succeeded = false
                };
            }
        }

        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync(); // Removes/invalidate the cookie
        //    return Ok(new { message = "Logout successful." });
        //}
    }
}
