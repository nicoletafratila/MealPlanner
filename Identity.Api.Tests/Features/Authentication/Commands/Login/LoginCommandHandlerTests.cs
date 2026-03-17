using Common.Models;
using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.Login
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private Mock<UserManager<Common.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<SignInManager<Common.Data.Entities.ApplicationUser>> _signInManagerMock = null!;
        private Mock<ILogger<LoginCommandHandler>> _loggerMock = null!;
        private LoginCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Common.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<Common.Data.Entities.ApplicationUser>>(
                _userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null);

            _loggerMock = new Mock<ILogger<LoginCommandHandler>>(MockBehavior.Loose);

            _handler = new LoginCommandHandler(
                _userManagerMock.Object,
                _signInManagerMock.Object,
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
                .ReturnsAsync((Common.Data.Entities.ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid credentials"));
            });

            _userManagerMock.Verify(m => m.FindByNameAsync("user"), Times.Once);
            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_SuccessfulLogin_ReturnsLoginCommandResponse()
        {
            // Arrange
            var user = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
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
                .Setup(s => s.PasswordSignInAsync("user", "pwd", true, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<LoginCommandResponse>());
            var loginResponse = (LoginCommandResponse)result!;
            Assert.Multiple(() =>
            {
                Assert.That(loginResponse.Succeeded, Is.True);
                Assert.That(loginResponse.JwtBearer, Is.Not.Null.And.Not.Empty);
                Assert.That(loginResponse.Claims, Is.Not.Empty);
            });

            _userManagerMock.Verify(m => m.FindByNameAsync("user"), Times.Once);
            _userManagerMock.Verify(m => m.GetRolesAsync(user), Times.Once);
            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "pwd", true, false),
                Times.Once);
        }

        [Test]
        public async Task Handle_LockedOut_ReturnsLockedOutMessage()
        {
            // Arrange
            var user = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
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
                .Setup(s => s.PasswordSignInAsync("user", "pwd", false, false))
                .ReturnsAsync(SignInResult.LockedOut);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User is locked out"));
            });

            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "pwd", false, false),
                Times.Once);
        }

        [Test]
        public async Task Handle_InvalidPassword_ReturnsUserPasswordNotFound()
        {
            // Arrange
            var user = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
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
                .Setup(s => s.PasswordSignInAsync("user", "wrong", false, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User/password not found."));
            });

            _signInManagerMock.Verify(
                s => s.PasswordSignInAsync("user", "wrong", false, false),
                Times.Once);
        }
    }
}