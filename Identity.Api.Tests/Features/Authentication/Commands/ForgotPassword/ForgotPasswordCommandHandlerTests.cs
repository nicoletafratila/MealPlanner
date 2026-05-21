using Identity.Api.Features.Authentication.Commands.ForgotPassword;
using Identity.Api.Services;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.ForgotPassword
{
    [TestFixture]
    public class ForgotPasswordCommandHandlerTests
    {
        private Mock<UserManager<Common.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock = null!;
        private ForgotPasswordCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Common.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _emailServiceMock = new Mock<IEmailService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>(MockBehavior.Loose);

            _handler = new ForgotPasswordCommandHandler(
                _userManagerMock.Object,
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
            var command = new ForgotPasswordCommand { Model = null };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_EmailNotRegistered_ReturnsSuccessWithoutSendingEmail()
        {
            _userManagerMock
                .Setup(m => m.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((Common.Data.Entities.ApplicationUser?)null);

            var command = BuildCommand("unknown@example.com");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _emailServiceMock.Verify(
                e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_EmailRegistered_SendsPasswordResetEmailAndReturnsSuccess()
        {
            var user = new Common.Data.Entities.ApplicationUser { Id = "user-id", Email = "user@example.com" };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _emailServiceMock
                .Setup(e => e.SendPasswordResetAsync("user@example.com", "user-id", "reset-token", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = BuildCommand("user@example.com");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _emailServiceMock.Verify(
                e => e.SendPasswordResetAsync("user@example.com", "user-id", "reset-token", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_EmailSendFails_StillReturnsSuccess()
        {
            var user = new Common.Data.Entities.ApplicationUser { Id = "user-id", Email = "user@example.com" };

            _userManagerMock
                .Setup(m => m.FindByEmailAsync("user@example.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token");

            _emailServiceMock
                .Setup(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP failure"));

            var command = BuildCommand("user@example.com");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
        }

        [Test]
        public async Task Handle_AlwaysReturnsSameSuccessMessage_RegardlessOfEmailExistence()
        {
            _userManagerMock
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Common.Data.Entities.ApplicationUser?)null);

            var resultForUnknown = await _handler.Handle(BuildCommand("nobody@example.com"), CancellationToken.None);

            var user = new Common.Data.Entities.ApplicationUser { Id = "uid", Email = "real@example.com" };
            _userManagerMock
                .Setup(m => m.FindByEmailAsync("real@example.com"))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(m => m.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("token");
            _emailServiceMock
                .Setup(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var resultForKnown = await _handler.Handle(BuildCommand("real@example.com"), CancellationToken.None);

            Assert.That(resultForUnknown!.Message, Is.EqualTo(resultForKnown!.Message));
        }

        private static ForgotPasswordCommand BuildCommand(string email = "user@example.com") =>
            new() { Model = new ForgotPasswordModel { EmailAddress = email } };
    }
}
