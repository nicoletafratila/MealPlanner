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

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var templateDir = Path.Combine(_tempDir, "EmailTemplates");
            Directory.CreateDirectory(templateDir);
            File.WriteAllText(Path.Combine(templateDir, "EmailConfirmation.html"), TemplateContent);

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
    }
}
