using System.Net;
using System.Net.Mail;
using Identity.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Services
{
    [TestFixture]
    public class SmtpEmailServiceTests
    {
        private Mock<IConfiguration> _configurationMock = null!;
        private Mock<IConfigurationSection> _emailSectionMock = null!;
        private Mock<IWebHostEnvironment> _environmentMock = null!;
        private Mock<ISmtpClientFactory> _smtpClientFactoryMock = null!;
        private Mock<ISmtpClient> _smtpClientMock = null!;
        private Mock<ILogger<SmtpEmailService>> _loggerMock = null!;
        private SmtpEmailService _service = null!;
        private string _tempDir = null!;

        private const string TemplateContent =
            "<html><head><title>{{EmailConfirmation_Title}}</title></head><body>" +
            "<h2>{{EmailConfirmation_Heading}}</h2>" +
            "<p>{{EmailConfirmation_Body}}</p>" +
            "<a href='{{ConfirmUrl}}'>{{EmailConfirmation_ButtonText}}</a>" +
            "<p>{{EmailConfirmation_FallbackText}}</p>" +
            "<a href='{{ConfirmUrl}}'>{{ConfirmUrl}}</a>" +
            "<p>{{EmailConfirmation_FooterText}}</p>" +
            "</body></html>";

        private const string ResetTemplateContent =
            "<html><head><title>{{PasswordReset_Title}}</title></head><body>" +
            "<h2>{{PasswordReset_Heading}}</h2>" +
            "<p>{{PasswordReset_Body}}</p>" +
            "<a href='{{ResetUrl}}'>{{PasswordReset_ButtonText}}</a>" +
            "<p>{{PasswordReset_FallbackText}}</p>" +
            "<a href='{{ResetUrl}}'>{{ResetUrl}}</a>" +
            "<p>{{PasswordReset_FooterText}}</p>" +
            "</body></html>";

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templateDir = Path.Combine(_tempDir, "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            File.WriteAllText(Path.Combine(templateDir, "EmailConfirmation.html"), TemplateContent);
            File.WriteAllText(Path.Combine(templateDir, "PasswordReset.html"), ResetTemplateContent);

            _emailSectionMock = new Mock<IConfigurationSection>(MockBehavior.Loose);
            _emailSectionMock.Setup(s => s["From"]).Returns("noreply@mealplanner.com");
            _emailSectionMock.Setup(s => s["Host"]).Returns("smtp.example.com");
            _emailSectionMock.Setup(s => s["Port"]).Returns("587");
            _emailSectionMock.Setup(s => s["EnableSsl"]).Returns("true");
            _emailSectionMock.Setup(s => s["Username"]).Returns("smtp-user");
            _emailSectionMock.Setup(s => s["Password"]).Returns("smtp-pass");

            _configurationMock = new Mock<IConfiguration>(MockBehavior.Loose);
            _configurationMock.Setup(c => c["IdentityApi:BaseUrl"]).Returns("https://localhost:5001");
            _configurationMock.Setup(c => c.GetSection("Email")).Returns(_emailSectionMock.Object);

            _environmentMock = new Mock<IWebHostEnvironment>(MockBehavior.Loose);
            _environmentMock.Setup(e => e.ContentRootPath).Returns(_tempDir);

            _smtpClientMock = new Mock<ISmtpClient>(MockBehavior.Loose);
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _smtpClientFactoryMock = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
            _smtpClientFactoryMock
                .Setup(f => f.Create("smtp.example.com", 587))
                .Returns(_smtpClientMock.Object);

            _loggerMock = new Mock<ILogger<SmtpEmailService>>(MockBehavior.Loose);

            _service = new SmtpEmailService(
                _configurationMock.Object,
                _environmentMock.Object,
                _smtpClientFactoryMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        [Test]
        public async Task SendEmailConfirmationAsync_SendsToCorrectRecipient()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.To[0].Address, Is.EqualTo("user@test.com"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_SetsFromAddressFromConfig()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.From!.Address, Is.EqualTo("noreply@mealplanner.com"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_SetsSubjectFromResources()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.Subject, Is.EqualTo("Confirm your email address"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_BodyContainsConfirmUrl()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            var expectedUrl = "https://localhost:5001/api/authentication/confirm-email?userId=user-id&token=token";
            Assert.That(captured!.Body, Does.Contain(expectedUrl));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_UrlEncodesToken()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "tok+en/with special=chars");

            Assert.That(captured!.Body, Does.Contain("token=tok%2Ben%2Fwith%20special%3Dchars"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_BodyContainsNoUnreplacedPlaceholders()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.Body, Does.Not.Contain("{{"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_CreatesSmtpClientWithConfiguredHostAndPort()
        {
            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            _smtpClientFactoryMock.Verify(f => f.Create("smtp.example.com", 587), Times.Once);
        }

        [Test]
        public async Task SendEmailConfirmationAsync_ConfiguresSmtpClientSslAndCredentials()
        {
            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            _smtpClientMock.VerifySet(c => c.EnableSsl = true);
            _smtpClientMock.VerifySet(c => c.Credentials = It.Is<NetworkCredential>(
                nc => nc.UserName == "smtp-user" && nc.Password == "smtp-pass"));
        }

        [Test]
        public async Task SendEmailConfirmationAsync_CallsSendMailAsync()
        {
            await _service.SendEmailConfirmationAsync("user@test.com", "user-id", "token");

            _smtpClientMock.Verify(
                c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SendPasswordResetAsync_SendsToCorrectRecipient()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.To[0].Address, Is.EqualTo("user@test.com"));
        }

        [Test]
        public async Task SendPasswordResetAsync_SetsFromAddressFromConfig()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.From!.Address, Is.EqualTo("noreply@mealplanner.com"));
        }

        [Test]
        public async Task SendPasswordResetAsync_SetsSubjectFromResources()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.Subject, Is.EqualTo("Reset your password"));
        }

        [Test]
        public async Task SendPasswordResetAsync_BodyContainsResetUrl()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            var expectedUrl = "https://localhost:5001/api/authentication/reset-password-redirect?userId=user-id&token=token";
            Assert.That(captured!.Body, Does.Contain(expectedUrl));
        }

        [Test]
        public async Task SendPasswordResetAsync_UrlEncodesToken()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "tok+en/with special=chars");

            Assert.That(captured!.Body, Does.Contain("token=tok%2Ben%2Fwith%20special%3Dchars"));
        }

        [Test]
        public async Task SendPasswordResetAsync_BodyContainsNoUnreplacedPlaceholders()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            Assert.That(captured!.Body, Does.Not.Contain("{{"));
        }

        [Test]
        public async Task SendPasswordResetAsync_CreatesSmtpClientWithConfiguredHostAndPort()
        {
            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            _smtpClientFactoryMock.Verify(f => f.Create("smtp.example.com", 587), Times.Once);
        }

        [Test]
        public async Task SendPasswordResetAsync_CallsSendMailAsync()
        {
            await _service.SendPasswordResetAsync("user@test.com", "user-id", "token");

            _smtpClientMock.Verify(
                c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SendContactUsAsync_SendsToFromAddress()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane Doe", "jane@example.com", "Hello", "Body");

            Assert.That(captured!.To[0].Address, Is.EqualTo("noreply@mealplanner.com"));
        }

        [Test]
        public async Task SendContactUsAsync_SetsReplyToSubmitterEmail()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane Doe", "jane@example.com", "Hello", "Body");

            Assert.That(captured!.ReplyToList[0].Address, Is.EqualTo("jane@example.com"));
            Assert.That(captured!.ReplyToList[0].DisplayName, Is.EqualTo("Jane Doe"));
        }

        [Test]
        public async Task SendContactUsAsync_SubjectContainsPrefix()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane", "jane@example.com", "My Subject", "Body");

            Assert.That(captured!.Subject, Does.Contain("Contact Us"));
            Assert.That(captured!.Subject, Does.Contain("My Subject"));
        }

        [Test]
        public async Task SendContactUsAsync_BodyContainsFromNameAndEmail()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane Doe", "jane@example.com", "Hello", "Test message");

            Assert.That(captured!.Body, Does.Contain("Jane Doe"));
            Assert.That(captured!.Body, Does.Contain("jane@example.com"));
        }

        [Test]
        public async Task SendContactUsAsync_BodyContainsMessage()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane", "jane@example.com", "Hello", "My test message content");

            Assert.That(captured!.Body, Does.Contain("My test message content"));
        }

        [Test]
        public async Task SendContactUsAsync_CreatesSmtpClientWithConfiguredHostAndPort()
        {
            await _service.SendContactUsAsync("Jane", "jane@example.com", "Hello", "Body");

            _smtpClientFactoryMock.Verify(f => f.Create("smtp.example.com", 587), Times.Once);
        }

        [Test]
        public async Task SendContactUsAsync_CallsSendMailAsync()
        {
            await _service.SendContactUsAsync("Jane", "jane@example.com", "Hello", "Body");

            _smtpClientMock.Verify(
                c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SendContactUsAsync_IsBodyHtml()
        {
            MailMessage? captured = null;
            _smtpClientMock
                .Setup(c => c.SendMailAsync(It.IsAny<MailMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MailMessage, CancellationToken>((msg, _) => captured = msg)
                .Returns(Task.CompletedTask);

            await _service.SendContactUsAsync("Jane", "jane@example.com", "Hello", "Body");

            Assert.That(captured!.IsBodyHtml, Is.True);
        }
    }
}
