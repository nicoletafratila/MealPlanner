using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Identity.Shared.Constants;
using Common.Models;
using Identity.Api;
using Identity.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace Identity.Services.Tests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string BaseAddress = "https://api.test/";
        private const string AuthPath = "api/auth";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static IdentityApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            return new IdentityApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [IdentityControllers.Authentication] = AuthPath
                }
            };
        }

        private static (AuthenticationService service, Mock<ISessionStorageService> storageMock) CreateService(
            MockHttpMessageHandler mockHttp)
        {
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri(BaseAddress)
            };

            var sessionStorage = new Mock<ISessionStorageService>();
            var tokenProvider = new TokenProvider(sessionStorage.Object);
            var config = CreateConfig();
            var logger = Mock.Of<ILogger<AuthenticationService>>();

            var service = new AuthenticationService(httpClient, tokenProvider, config, logger);
            return (service, sessionStorage);
        }

        // ---------- LoginAsync ----------
        [Test]
        public async Task LoginAsync_Succeeds_SetsToken_AndReturnsResponse()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "user", Password = "pass" };

            var loginResponse = new LoginCommandResponse
            {
                Succeeded = true,
                JwtBearer = "jwt-token",
                Message = "ok"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/login")
                .Respond("application/json", JsonSerializer.Serialize(loginResponse, JsonOptions));

            var (service, storageMock) = CreateService(mockHttp);

            storageMock
                .Setup(s => s.SetItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("ok"));
            }
            storageMock.Verify(
                s => s.SetItemAsync(Common.Constants.MealPlanner.AuthToken, "jwt-token", It.IsAny<CancellationToken>()),
                Times.Once);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task LoginAsync_DeserializedButFailed_ReturnsFailedCommandResponse()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "user", Password = "pass" };

            var loginResponse = new LoginCommandResponse
            {
                Succeeded = false,
                JwtBearer = null,
                Message = "invalid credentials"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/login")
                .Respond("application/json", JsonSerializer.Serialize(loginResponse, JsonOptions));

            var (service, storageMock) = CreateService(mockHttp);

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("invalid credentials"));
            }
            storageMock.Verify(
                s => s.SetItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task LoginAsync_NonSuccessStatusCode_UsesErrorBody_AsMessage()
        {
            // Arrange
            var loginModel = new LoginModel { Username = "user", Password = "pass" };
            const string errorBody = "Bad credentials";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/login")
                .Respond(HttpStatusCode.BadRequest, "text/plain", errorBody);

            var (service, storageMock) = CreateService(mockHttp);

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo(errorBody));
            }
            storageMock.Verify(
                s => s.SetItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- LogoutAsync ----------
        [Test]
        public async Task LogoutAsync_Succeeds_RemovesToken_AndReturnsResponse()
        {
            // Arrange
            var logoutResponse = new CommandResponse
            {
                Succeeded = true,
                Message = "logged out"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/logout")
                .Respond("application/json", JsonSerializer.Serialize(logoutResponse, JsonOptions));

            var (service, storageMock) = CreateService(mockHttp);

            storageMock
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync("existing-token");

            storageMock
                .Setup(s => s.RemoveItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            // Act
            var result = await service.LogoutAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("logged out"));
            }
            storageMock.Verify(
                s => s.RemoveItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()),
                Times.Once);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task LogoutAsync_NonSuccessStatusCode_UsesErrorBody_AsMessage()
        {
            // Arrange
            const string errorBody = "logout error";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/logout")
                .Respond(HttpStatusCode.BadRequest, "text/plain", errorBody);

            var (service, storageMock) = CreateService(mockHttp);

            storageMock
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync("existing-token");

            // Act
            var result = await service.LogoutAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo(errorBody));
            }
            storageMock.Verify(
                s => s.RemoveItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()),
                Times.Never);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task LogoutAsync_DeserializedButFailed_ReturnsFailedCommandResponse()
        {
            // Arrange
            var logoutResponse = new CommandResponse
            {
                Succeeded = false,
                Message = "logout failed"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/logout")
                .Respond("application/json", JsonSerializer.Serialize(logoutResponse, JsonOptions));

            var (service, storageMock) = CreateService(mockHttp);

            storageMock
                .Setup(s => s.GetItemAsync<string?>(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync("existing-token");

            // Act
            var result = await service.LogoutAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("logout failed"));
            }
            storageMock.Verify(
                s => s.RemoveItemAsync(Common.Constants.MealPlanner.AuthToken, It.IsAny<CancellationToken>()),
                Times.Never);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- RegisterAsync ----------
        [Test]
        public async Task RegisterAsync_Succeeds_ReturnsResponse()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Username = "user",
                Password = "pass",
                ConfirmPassword = "pass"
            };

            var registerResponse = new CommandResponse
            {
                Succeeded = true,
                Message = "registered"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/register")
                .Respond("application/json", JsonSerializer.Serialize(registerResponse, JsonOptions));

            var (service, _) = CreateService(mockHttp);

            // Act
            var result = await service.RegisterAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("registered"));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task RegisterAsync_NonSuccessStatusCode_UsesErrorBody_AsMessage()
        {
            // Arrange
            var model = new RegistrationModel
            {
                Username = "user",
                Password = "pass",
                ConfirmPassword = "pass"
            };

            const string errorBody = "registration error";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/register")
                .Respond(HttpStatusCode.BadRequest, "text/plain", errorBody);

            var (service, _) = CreateService(mockHttp);

            // Act
            var result = await service.RegisterAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo(errorBody));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- ForgotPasswordAsync ----------
        [Test]
        public async Task ForgotPasswordAsync_Succeeds_ReturnsResponse()
        {
            var model = new ForgotPasswordModel { EmailAddress = "user@example.com" };
            var serverResponse = new CommandResponse { Succeeded = true, Message = "If an account exists, a link was sent." };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/forgot-password")
                .Respond("application/json", JsonSerializer.Serialize(serverResponse, JsonOptions));

            var (service, _) = CreateService(mockHttp);

            var result = await service.ForgotPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("If an account exists, a link was sent."));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ForgotPasswordAsync_NonSuccessStatusCode_ReturnsFailure()
        {
            var model = new ForgotPasswordModel { EmailAddress = "user@example.com" };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/forgot-password")
                .Respond(HttpStatusCode.InternalServerError, "text/plain", "server error");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ForgotPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ForgotPasswordAsync_NullDeserializationResult_ReturnsFailure()
        {
            var model = new ForgotPasswordModel { EmailAddress = "user@example.com" };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/forgot-password")
                .Respond("application/json", "null");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ForgotPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- ResetPasswordAsync ----------
        [Test]
        public async Task ResetPasswordAsync_Succeeds_ReturnsResponse()
        {
            var model = new ResetPasswordModel
            {
                UserId = "user-id",
                Token = "reset-token",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };
            var serverResponse = new CommandResponse { Succeeded = true, Message = "Password reset successfully." };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/reset-password")
                .Respond("application/json", JsonSerializer.Serialize(serverResponse, JsonOptions));

            var (service, _) = CreateService(mockHttp);

            var result = await service.ResetPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("Password reset successfully."));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ResetPasswordAsync_NonSuccessStatusCode_ReturnsFailure()
        {
            var model = new ResetPasswordModel
            {
                UserId = "user-id",
                Token = "bad-token",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/reset-password")
                .Respond(HttpStatusCode.BadRequest, "text/plain", "invalid token");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ResetPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ResetPasswordAsync_NullDeserializationResult_ReturnsFailure()
        {
            var model = new ResetPasswordModel
            {
                UserId = "user-id",
                Token = "reset-token",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/reset-password")
                .Respond("application/json", "null");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ResetPasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- ChangePasswordAsync ----------
        [Test]
        public async Task ChangePasswordAsync_Succeeds_ReturnsResponse()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };
            var serverResponse = new CommandResponse { Succeeded = true, Message = "Password changed successfully." };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/change-password")
                .Respond("application/json", JsonSerializer.Serialize(serverResponse, JsonOptions));

            var (service, _) = CreateService(mockHttp);

            var result = await service.ChangePasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("Password changed successfully."));
            }
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangePasswordAsync_NonSuccessStatusCode_ReturnsFailure()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "wrong-pass",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/change-password")
                .Respond(HttpStatusCode.BadRequest, "text/plain", "Incorrect password.");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ChangePasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ChangePasswordAsync_NullDeserializationResult_ReturnsFailure()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{AuthPath}/change-password")
                .Respond("application/json", "null");

            var (service, _) = CreateService(mockHttp);

            var result = await service.ChangePasswordAsync(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.False);
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}
