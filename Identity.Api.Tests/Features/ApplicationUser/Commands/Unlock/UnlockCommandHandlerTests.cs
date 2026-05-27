using Identity.Api.Features.ApplicationUser.Commands.Unlock;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Unlock
{
    [TestFixture]
    public class UnlockCommandHandlerTests
    {
        private Mock<UserManager<Identity.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<ILogger<UnlockCommandHandler>> _loggerMock = null!;
        private UnlockCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Identity.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Identity.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<UnlockCommandHandler>>(MockBehavior.Loose);

            _handler = new UnlockCommandHandler(_userManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullUserManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UnlockCommandHandler(null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UnlockCommandHandler(_userManagerMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EmptyUserId_ReturnsFailure()
        {
            var command = new UnlockCommand { UserId = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullUserId_ReturnsFailure()
        {
            var command = new UnlockCommand { UserId = null };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            var command = new UnlockCommand { UserId = "unknown-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("unknown-id"))
                .ReturnsAsync((Identity.Data.Entities.ApplicationUser?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync("unknown-id"), Times.Once);
            _userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(It.IsAny<Identity.Data.Entities.ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task Handle_Success_ResetsCountAndClearsLockout_AndReturnsSuccess()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-1", UserName = "alice" };
            var command = new UnlockCommand { UserId = "user-1" };

            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(m => m.SetLockoutEndDateAsync(user, null)).ReturnsAsync(IdentityResult.Success);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _userManagerMock.Verify(m => m.ResetAccessFailedCountAsync(user), Times.Once);
            _userManagerMock.Verify(m => m.SetLockoutEndDateAsync(user, null), Times.Once);
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsFailure()
        {
            var command = new UnlockCommand { UserId = "user-1" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-1"))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
