using System.Security.Claims;
using Common.Data.Repository;
using Identity.Api.Features.Authentication.Commands.Register;
using Identity.Api.Features.Email;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Data.Entities;

namespace Identity.Api.Tests.Features.Authentication.Commands.Register
{
    [TestFixture]
    public class RegisterCommandHandlerTests
    {
        private Mock<UserManager<Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IAsyncRepository<ProductCategory, Guid>> _productCategoryRepoMock = null!;
        private Mock<IAsyncRepository<RecipeCategory, Guid>> _recipeCategoryRepoMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<ILogger<RegisterCommandHandler>> _loggerMock = null!;
        private RegisterCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _productCategoryRepoMock = new Mock<IAsyncRepository<ProductCategory, Guid>>(MockBehavior.Loose);
            _recipeCategoryRepoMock = new Mock<IAsyncRepository<RecipeCategory, Guid>>(MockBehavior.Loose);
            _emailServiceMock = new Mock<IEmailService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<RegisterCommandHandler>>(MockBehavior.Loose);

            _handler = new RegisterCommandHandler(
                _userManagerMock.Object,
                _productCategoryRepoMock.Object,
                _recipeCategoryRepoMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public void Handle_NullModel_ThrowsArgumentNullException()
        {
            var command = new RegisterCommand { Model = null! };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UsernameTaken_ReturnsFailedResponse()
        {
            var command = BuildCommand();

            _userManagerMock
                .Setup(m => m.FindByNameAsync("newuser"))
                .ReturnsAsync(new Data.Entities.ApplicationUser { UserName = "newuser" });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Username is already taken."));
            }

            _userManagerMock.Verify(m => m.FindByNameAsync("newuser"), Times.Once);
            _userManagerMock.Verify(
                m => m.CreateAsync(It.IsAny<Data.Entities.ApplicationUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_EmailTaken_ReturnsFailedResponse()
        {
            var command = BuildCommand();

            _userManagerMock
                .Setup(m => m.FindByNameAsync("newuser"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync(new Data.Entities.ApplicationUser { Email = "newuser@example.com" });

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Email address is already registered."));
            }

            _userManagerMock.Verify(m => m.FindByEmailAsync("newuser@example.com"), Times.Once);
            _userManagerMock.Verify(
                m => m.CreateAsync(It.IsAny<Data.Entities.ApplicationUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_CreateUserFails_ReturnsFailedResponse()
        {
            var command = BuildCommand();

            _userManagerMock
                .Setup(m => m.FindByNameAsync("newuser"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<Data.Entities.ApplicationUser>(), "Test123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.Contain("Password too weak."));
            }

            _userManagerMock.Verify(
                m => m.AddToRoleAsync(It.IsAny<Data.Entities.ApplicationUser>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_Success_SeedsUserCategoriesAndReturnsSuccess()
        {
            var command = BuildCommand();

            var productTemplates = new List<ProductCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "Vegetables", UserId = null },
                new() { Id = Guid.NewGuid(), Name = "Dairy", UserId = null },
                new() { Id = Guid.NewGuid(), Name = "OtherUserOwned", UserId = "other-user" }
            };

            var recipeTemplates = new List<RecipeCategory>
            {
                new() { Id = Guid.NewGuid(), Name = "Breakfast", DisplaySequence = 1, UserId = null },
                new() { Id = Guid.NewGuid(), Name = "Dinner", DisplaySequence = 2, UserId = null }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("newuser"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("newuser@example.com"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<Data.Entities.ApplicationUser>(), "Test123!"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddToRoleAsync(It.IsAny<Data.Entities.ApplicationUser>(), "member"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.AddClaimsAsync(It.IsAny<Data.Entities.ApplicationUser>(), It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<Data.Entities.ApplicationUser>()))
                .ReturnsAsync("confirmation-token");

            _emailServiceMock
                .Setup(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _productCategoryRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productTemplates);

            _productCategoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductCategory c, CancellationToken _) => c);

            _recipeCategoryRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(recipeTemplates);

            _recipeCategoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<RecipeCategory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RecipeCategory c, CancellationToken _) => c);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("Registration successful. Please check your email to confirm your account."));
            }

            // Only null-UserId (seeded) templates get copied — 2 products, 2 recipes
            _productCategoryRepoMock.Verify(
                r => r.AddAsync(It.Is<ProductCategory>(c => c.UserId != null), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            _recipeCategoryRepoMock.Verify(
                r => r.AddAsync(It.Is<RecipeCategory>(c => c.UserId != null), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            _userManagerMock.Verify(
                m => m.AddToRoleAsync(It.IsAny<Data.Entities.ApplicationUser>(), "member"),
                Times.Once);

            _userManagerMock.Verify(
                m => m.CreateAsync(
                    It.Is<Data.Entities.ApplicationUser>(u => !u.IsActive && !u.EmailConfirmed),
                    It.IsAny<string>()),
                Times.Once);

            _emailServiceMock.Verify(
                e => e.SendEmailConfirmationAsync("newuser@example.com", It.IsAny<string>(), "confirmation-token", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private static RegisterCommand BuildCommand() =>
            new()
            {
                Model = new RegistrationModel
                {
                    Username = "newuser",
                    Password = "Test123!",
                    ConfirmPassword = "Test123!",
                    EmailAddress = "newuser@example.com",
                    FirstName = "New",
                    LastName = "User"
                }
            };
    }
}
