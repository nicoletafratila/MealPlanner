using System.Net;
using System.Text.Json;
using Common.Models;
using Identity.Services.Http;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace Identity.Services.Http.Tests
{
    [TestFixture]
    public class ContactUsServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string ContactUsPath = "api/contactus";
        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static (ContactUsService service, MockHttpMessageHandler mockHttp) CreateService()
        {
            var mockHttp = new MockHttpMessageHandler();
            var httpClient = new HttpClient(mockHttp) { BaseAddress = new Uri(BaseAddress) };
            var tokenProvider = Mock.Of<Common.Http.ITokenProvider>();
            var logger = Mock.Of<ILogger<ContactUsService>>();
            var service = new ContactUsService(httpClient, tokenProvider, logger);
            return (service, mockHttp);
        }

        private static ContactUsModel ValidModel() => new()
        {
            Name = "Jane",
            EmailAddress = "jane@example.com",
            Subject = "Hello",
            Message = "Body"
        };

        [Test]
        public async Task SendAsync_SuccessResponse_ReturnsCommandResponse()
        {
            var expected = new CommandResponse { Succeeded = true, Message = "Your message has been sent. We will get back to you shortly." };
            var (service, mockHttp) = CreateService();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}api/contactus/send")
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var result = await service.SendAsync(ValidModel());

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo(expected.Message));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SendAsync_NonSuccessStatusCode_ReturnsFailedResponse()
        {
            var (service, mockHttp) = CreateService();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}api/contactus/send")
                .Respond(HttpStatusCode.InternalServerError, "text/plain", "error");

            var result = await service.SendAsync(ValidModel());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SendAsync_NullDeserialization_ReturnsFailedResponse()
        {
            var (service, mockHttp) = CreateService();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}api/contactus/send")
                .Respond("application/json", "null");

            var result = await service.SendAsync(ValidModel());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SendAsync_NetworkError_ReturnsFailedResponse()
        {
            var (service, mockHttp) = CreateService();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}api/contactus/send")
                .Throw(new HttpRequestException("connection refused"));

            var result = await service.SendAsync(ValidModel());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task SendAsync_InvalidJson_ReturnsFailedResponse()
        {
            var (service, mockHttp) = CreateService();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}api/contactus/send")
                .Respond("application/json", "not-valid-json{{{");

            var result = await service.SendAsync(ValidModel());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
