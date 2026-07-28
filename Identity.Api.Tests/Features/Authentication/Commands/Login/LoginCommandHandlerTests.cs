using Common.Data.DataContext;
using Common.Models;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Data.TableConfigurations;
using Identity.Shared.Models;
using MealPlanner.Data.TableConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Data.TableConfigurations;

namespace Identity.Api.Tests.Features.Authentication.Commands.Login
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private Mock<UserManager<Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<SignInManager<Data.Entities.ApplicationUser>> _signInManagerMock = null!;
        private Mock<ILogger<LoginCommandHandler>> _loggerMock = null!;
        private MealPlannerDbContext _dbContext = null!;
        private LoginCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<Data.Entities.ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<Data.Entities.ApplicationUser>>(),
                null, null, null, null);

            _loggerMock = new Mock<ILogger<LoginCommandHandler>>(MockBehavior.Loose);

            var tableConfigurationAssemblies = new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly,
                typeof(RefreshTokenTableConfiguration).Assembly
            ]);
            _dbContext = new MealPlannerDbContext(
                new DbContextOptionsBuilder<MealPlannerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                tableConfigurationAssemblies);

            _handler = new LoginCommandHandler(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _dbContext,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown() => _dbContext.Dispose();

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public void Handle_NullModel_ThrowsArgumentNullException()
        {
            var command = new LoginCommand { Model = null! };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsInvalidCredentials()
        {
            // Arrange
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "pwd" }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid credentials."));
            }

            _userManagerMock.Verify(m => m.FindByNameAsync("user"), Times.Once);
            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_UserNotActive_ReturnsUserNotActiveMessage()
        {
            // Arrange
            var user = new Data.Entities.ApplicationUser { Id = "1", UserName = "user", IsActive = false };
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "pwd" }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Your account is not active. Please contact an administrator."));
            }

            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulLogin_WithRememberLogin_IssuesRefreshToken()
        {
            // Arrange
            var user = new Data.Entities.ApplicationUser { Id = "1", UserName = "user", IsActive = true };
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "pwd", RememberLogin = true }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(["admin"]);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("user", "pwd", true, true))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<LoginCommandResponse>());
            var loginResponse = (LoginCommandResponse)result!;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(loginResponse.Succeeded, Is.True);
                Assert.That(loginResponse.JwtBearer, Is.Not.Null.And.Not.Empty);
                Assert.That(loginResponse.RefreshToken, Is.Not.Null.And.Not.Empty);
                Assert.That(loginResponse.Claims, Is.Not.Empty);
            }

            var storedTokens = await _dbContext.RefreshTokens.ToListAsync();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(storedTokens, Has.Count.EqualTo(1));
                Assert.That(storedTokens[0].UserId, Is.EqualTo("1"));
                Assert.That(storedTokens[0].IsActive, Is.True);
            }

            _userManagerMock.Verify(m => m.FindByNameAsync("user"), Times.Once);
            _userManagerMock.Verify(m => m.GetRolesAsync(user), Times.Once);
            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "pwd", true, true),
                Times.Once);
        }

        [Test]
        public async Task Handle_SuccessfulLogin_WithoutRememberLogin_DoesNotIssueRefreshToken()
        {
            // Arrange
            var user = new Data.Entities.ApplicationUser { Id = "1", UserName = "user", IsActive = true };
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "pwd", RememberLogin = false }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(["admin"]);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("user", "pwd", false, true))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var loginResponse = (LoginCommandResponse)result!;
            Assert.That(loginResponse.RefreshToken, Is.Null);

            var storedTokens = await _dbContext.RefreshTokens.ToListAsync();
            Assert.That(storedTokens, Is.Empty);
        }

        [Test]
        public async Task Handle_LockedOut_ReturnsLockedOutMessage()
        {
            // Arrange
            var user = new Data.Entities.ApplicationUser { Id = "1", UserName = "user", IsActive = true };
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "pwd" }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync([]);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("user", "pwd", false, true))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User is locked out"));
            }

            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "pwd", false, true),
                Times.Once);
        }

        [Test]
        public async Task Handle_InvalidPassword_ReturnsUserPasswordNotFound()
        {
            // Arrange
            var user = new Data.Entities.ApplicationUser { Id = "1", UserName = "user", IsActive = true };
            var command = new LoginCommand
            {
                Model = new LoginModel { Username = "user", Password = "wrong" }
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("user"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync([]);

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync("user", "wrong", false, true))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User/password not found."));
            }

            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "wrong", false, true),
                Times.Once);
        }
    }
}