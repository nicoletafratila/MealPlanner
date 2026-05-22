using Common.Models;
using Identity.Api.Features.ContactUs.Commands.Send;
using Identity.Api.Services;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.ContactUs.Commands.Send
{
    [TestFixture]
    public class SendContactUsCommandHandlerTests
    {
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<ILogger<SendContactUsCommandHandler>> _loggerMock = null!;
        private SendContactUsCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _emailServiceMock = new Mock<IEmailService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<SendContactUsCommandHandler>>(MockBehavior.Loose);
            _handler = new SendContactUsCommandHandler(_emailServiceMock.Object, _loggerMock.Object);
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
            var command = new SendContactUsCommand { Model = null };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ValidModel_CallsSendContactUsAsync()
        {
            _emailServiceMock
                .Setup(e => e.SendContactUsAsync("Jane", "jane@example.com", "Hello", "Body", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

            _emailServiceMock.Verify(
                e => e.SendContactUsAsync("Jane", "jane@example.com", "Hello", "Body", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task Handle_ValidModel_ReturnsSuccess()
        {
            _emailServiceMock
                .Setup(e => e.SendContactUsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
        }

        [Test]
        public async Task Handle_EmailSendFails_ReturnsFailure()
        {
            _emailServiceMock
                .Setup(e => e.SendContactUsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("SMTP down"));

            var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
        }

        private static SendContactUsCommand BuildCommand() =>
            new()
            {
                Model = new ContactUsModel
                {
                    Name = "Jane",
                    EmailAddress = "jane@example.com",
                    Subject = "Hello",
                    Message = "Body"
                }
            };
    }
}
