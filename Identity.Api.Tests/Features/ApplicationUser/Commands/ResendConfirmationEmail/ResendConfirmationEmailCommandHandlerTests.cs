using Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail;
using Identity.Api.Features.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    [TestFixture]
    public class ResendConfirmationEmailCommandHandlerTests
    {
        private Mock<UserManager<Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<ILogger<ResendConfirmationEmailCommandHandler>> _loggerMock = null!;
        private ResendConfirmationEmailCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _emailServiceMock = new Mock<IEmailService>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ResendConfirmationEmailCommandHandler>>(MockBehavior.Loose);

            _handler = new ResendConfirmationEmailCommandHandler(_userManagerMock.Object, _emailServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullUserManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ResendConfirmationEmailCommandHandler(null!, _emailServiceMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullEmailService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ResendConfirmationEmailCommandHandler(_userManagerMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ResendConfirmationEmailCommandHandler(_userManagerMock.Object, _emailServiceMock.Object, null!));
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
            var command = new ResendConfirmationEmailCommand { UserId = "" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Handle_NullUserId_ReturnsFailure()
        {
            var command = new ResendConfirmationEmailCommand { UserId = null };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "unknown-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("unknown-id"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.FindByIdAsync("unknown-id"), Times.Once);
            _userManagerMock.Verify(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<Data.Entities.ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task Handle_EmailAlreadyConfirmed_ReturnsFailure()
        {
            var user = new Data.Entities.ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@test.com", EmailConfirmed = true };
            var command = new ResendConfirmationEmailCommand { UserId = "user-1" };

            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);

            _userManagerMock.Verify(m => m.GenerateEmailConfirmationTokenAsync(user), Times.Never);
        }

        [Test]
        public async Task Handle_Success_GeneratesTokenAndSendsEmail_AndReturnsSuccess()
        {
            var user = new Data.Entities.ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@test.com", EmailConfirmed = false };
            var command = new ResendConfirmationEmailCommand { UserId = "user-1" };

            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("token-abc");
            _emailServiceMock
                .Setup(e => e.SendEmailConfirmationAsync("alice@test.com", "user-1", "token-abc", CancellationToken.None))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _emailServiceMock.Verify(
                e => e.SendEmailConfirmationAsync("alice@test.com", "user-1", "token-abc", CancellationToken.None),
                Times.Once);
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsFailure()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "user-1" };

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
