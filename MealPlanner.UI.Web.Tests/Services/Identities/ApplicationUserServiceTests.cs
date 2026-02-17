using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Common.Constants;
using Common.Models;
using Identity.Shared.Models;
using MealPlanner.UI.Web.Services.Identities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace MealPlanner.UI.Web.Tests.Services.Identities
{
    [TestFixture]
    public class ApplicationUserServiceTests
    {
        private const string AuthTokenKey = Common.Constants.MealPlanner.AuthToken;
        private const string BaseAddress = "https://api.test/";
        private const string UserPath = "api/user";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static IdentityApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            return new IdentityApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [IdentityControllers.ApplicationUser] = UserPath
                }
            };
        }

        private static ApplicationUserService CreateService(
            MockHttpMessageHandler mockHttp,
            Mock<ISessionStorageService>? storageMockOut = null,
            string token = "test-token")
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var sessionStorage = new Mock<ISessionStorageService>();
            sessionStorage
                .Setup(s => s.GetItemAsync<string?>(AuthTokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            storageMockOut?.SetReturnsDefault(sessionStorage.Object);

            var tokenProvider = new TokenProvider(sessionStorage.Object);
            var config = CreateConfig();
            var logger = Mock.Of<ILogger<ApplicationUserService>>();

            return new ApplicationUserService(httpClient, tokenProvider, config, logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            const string userName = "john.doe@example.com";
            var expected = new ApplicationUserEditModel { Username = userName };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UserPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    var query = m.RequestUri!.Query;

                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           // Just check the value, do not assume encoding form
                           && query.Contains($"username={userName}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token: token);

            // Act
            var result = await service.GetEditAsync(userName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Username, Is.EqualTo(expected.Username));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        [Test]
        public void GetEditAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            const string userName = "john.doe@example.com";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{UserPath}/edit*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetEditAsync(userName));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_OnSuccess_DeserializesAndReturnsResponse()
        {
            // Arrange
            var model = new ApplicationUserEditModel { Username = "john" };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UserPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ApplicationUserEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Username == model.Username;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("ok"));
            });
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_OnNonSuccessStatusCode_ReturnsFailedWithErrorBody()
        {
            // Arrange
            var model = new ApplicationUserEditModel { Username = "john" };
            const string errorBody = "update error";

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UserPath}")
                .Respond(HttpStatusCode.BadRequest, "text/plain", errorBody);

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo(errorBody));
            });
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task UpdateAsync_OnInvalidJson_ReturnsFailedInvalidResponse()
        {
            // Arrange
            var model = new ApplicationUserEditModel { Username = "john" };

            var mockHttp = new MockHttpMessageHandler();

            // Return invalid JSON so deserialization fails
            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{UserPath}")
                .Respond("application/json", "{ invalid json ");

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid response from user update endpoint."));
            });
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}