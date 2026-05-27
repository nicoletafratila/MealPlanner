using System.Security.Claims;
using Identity.Data.Entities;
using RecipeBook.Data.Entities;
using Common.Data.Repository;
using Common.Models;
using Duende.IdentityModel;
using Identity.Api.Features.Authentication.Resources;
using Identity.Api.Features.Email;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler(
        UserManager<Identity.Data.Entities.ApplicationUser> userManager,
        IAsyncRepository<ProductCategory, int> productCategoryRepository,
        IAsyncRepository<RecipeCategory, int> recipeCategoryRepository,
        IEmailService emailService,
        ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, CommandResponse?>
    {
        private const string MemberRoleName = "member";

        public async Task<CommandResponse?> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), AuthenticationMessages.ModelCannotBeNull);

            try
            {
                var model = request.Model;

                var existingByName = await userManager.FindByNameAsync(model.Username);
                if (existingByName is not null)
                    return CommandResponse.Failed(AuthenticationMessages.UsernameAlreadyTaken);

                var existingByEmail = await userManager.FindByEmailAsync(model.EmailAddress);
                if (existingByEmail is not null)
                    return CommandResponse.Failed(AuthenticationMessages.EmailAlreadyTaken);

                var user = new Identity.Data.Entities.ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.EmailAddress,
                    EmailConfirmed = false,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = false
                };

                var createResult = await userManager.CreateAsync(user, model.Password!);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create user {Username}: {Errors}", model.Username, errors);
                    return CommandResponse.Failed(errors);
                }

                await userManager.AddToRoleAsync(user, MemberRoleName);

                await userManager.AddClaimsAsync(user,
                [
                    new Claim(JwtClaimTypes.Subject, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtClaimTypes.GivenName, user.FirstName ?? string.Empty),
                    new Claim(JwtClaimTypes.FamilyName, user.LastName ?? string.Empty),
                ]);

                await SeedUserCategoriesAsync(user.Id, cancellationToken);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                try
                {
                    await emailService.SendEmailConfirmationAsync(user.Email!, user.Id, token, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                }

                logger.LogDebug("User {Username} registered successfully", user.UserName);
                return CommandResponse.Success(AuthenticationMessages.RegistrationSuccessful);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during registration for user '{Username}'.", request.Model.Username);
                return CommandResponse.Failed(AuthenticationMessages.RegistrationError);
            }
        }

        private async Task SeedUserCategoriesAsync(string userId, CancellationToken cancellationToken)
        {
            var allProductCategories = await productCategoryRepository.GetAllAsync(cancellationToken);
            foreach (var template in allProductCategories.Where(c => c.UserId == null))
            {
                await productCategoryRepository.AddAsync(
                    new ProductCategory { Name = template.Name, UserId = userId },
                    cancellationToken);
            }

            var allRecipeCategories = await recipeCategoryRepository.GetAllAsync(cancellationToken);
            foreach (var template in allRecipeCategories.Where(c => c.UserId == null))
            {
                await recipeCategoryRepository.AddAsync(
                    new RecipeCategory { Name = template.Name, DisplaySequence = template.DisplaySequence, UserId = userId },
                    cancellationToken);
            }
        }
    }
}
