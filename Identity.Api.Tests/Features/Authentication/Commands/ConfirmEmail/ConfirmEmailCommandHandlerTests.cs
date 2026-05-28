using Identity.Api.Features.Authentication.Commands.ConfirmEmail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.ConfirmEmail
{
    [TestFixture]
    public class ConfirmEmailCommandHandlerTests
    {
        private Mock<UserManager<Identity.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<ILogger<ConfirmEmailCommandHandler>> _loggerMock = null!;
        private ConfirmEmailCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Identity.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Identity.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<ConfirmEmailCommandHandler>>(MockBehavior.Loose);

            _handler = new ConfirmEmailCommandHandler(_userManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailedResponse()
        {
            _userManagerMock
                .Setup(m => m.FindByIdAsync("unknown-id"))
                .ReturnsAsync((Identity.Data.Entities.ApplicationUser?)null);

            var result = await _handler.Handle(new ConfirmEmailCommand { UserId = "unknown-id", Token = "token" }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid email confirmation link."));
            }
        }

        [Test]
        public async Task Handle_InvalidToken_ReturnsFailedResponse()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-id", UserName = "testuser" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ConfirmEmailAsync(user, "bad-token"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            var result = await _handler.Handle(new ConfirmEmailCommand { UserId = "user-id", Token = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Email confirmation failed. The link may be invalid or expired."));
            }

            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<Identity.Data.Entities.ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task Handle_Success_ActivatesUserAndReturnsSuccess()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-id", UserName = "testuser", IsActive = false };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ConfirmEmailAsync(user, "valid-token"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<Identity.Data.Entities.ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _handler.Handle(new ConfirmEmailCommand { UserId = "user-id", Token = "valid-token" }, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("Your email has been confirmed. You can now log in."));
            }

            Assert.That(user.IsActive, Is.True);
            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }
    }
}
