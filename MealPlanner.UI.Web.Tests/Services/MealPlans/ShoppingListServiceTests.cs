using System.Net;
using System.Text.Json;
using Blazored.SessionStorage;
using Common.Api;
using Common.Constants;
using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services.MealPlans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;

namespace MealPlanner.UI.Web.Tests.Services.MealPlans
{
    [TestFixture]
    public class ShoppingListServiceTests
    {
        private const string AuthTokenKey = Common.Constants.MealPlanner.AuthToken;
        private const string BaseAddress = "https://api.test/";
        private const string ShoppingListPath = "api/shoppinglist";

        private static JsonSerializerOptions JsonOptions => new(JsonSerializerDefaults.Web);

        private static MealPlannerApiConfig CreateConfig()
        {
            var configuration = new ConfigurationBuilder().Build();
            return new MealPlannerApiConfig(configuration)
            {
                BaseUrl = new Uri(BaseAddress),
                Controllers = new Dictionary<string, string>
                {
                    [MealPlannerControllers.ShoppingList] = ShoppingListPath
                }
            };
        }

        private static ShoppingListService CreateService(
            MockHttpMessageHandler mockHttp,
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

            var tokenProvider = new TokenProvider(sessionStorage.Object);
            var config = CreateConfig();
            var logger = Mock.Of<ILogger<ShoppingListService>>();

            return new ShoppingListService(httpClient, tokenProvider, config, logger);
        }

        // ---------- GetEditAsync ----------
        [Test]
        public async Task GetEditAsync_ReturnsDeserializedModel_AndSendsAuthHeader()
        {
            // Arrange
            const string token = "my-jwt-token";
            var id = 42;
            var expected = new ShoppingListEditModel { Id = id };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShoppingListPath}/edit*")
                .With(m =>
                {
                    var auth = m.Headers.Authorization;
                    return auth is not null
                           && auth.Scheme == JwtBearerDefaults.AuthenticationScheme
                           && auth.Parameter == token
                           && m.RequestUri!.Query.Contains($"id={id}");
                })
                .Respond("application/json", JsonSerializer.Serialize(expected, JsonOptions));

            var service = CreateService(mockHttp, token);

            // Act
            var result = await service.GetEditAsync(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expected.Id));
            mockHttp.VerifyNoOutstandingExpectation();
            mockHttp.VerifyNoOutstandingRequest();
        }

        // ---------- SearchAsync ----------
        [Test]
        public async Task SearchAsync_DeserializesPagedList_OnSuccess()
        {
            // Arrange
            var metadata = new Metadata
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2
            };

            var paged = new PagedList<ShoppingListModel>(
                [
                    new ShoppingListModel(),
                    new ShoppingListModel()
                ],
                metadata);

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShoppingListPath}/search*")
                .Respond("application/json", JsonSerializer.Serialize(paged, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.SearchAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Items, Has.Count.EqualTo(2));
                Assert.That(result.Metadata.PageNumber, Is.EqualTo(1));
            });
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void SearchAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Get, $"{BaseAddress}{ShoppingListPath}/search*")
                .Respond(HttpStatusCode.InternalServerError);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.SearchAsync());
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- MakeShoppingListAsync ----------
        [Test]
        public async Task MakeShoppingListAsync_PostsCreateModel_AndReturnsEditModel()
        {
            // Arrange
            var createModel = new ShoppingListCreateModel
            {
                // populate properties as needed
            };

            var expectedEdit = new ShoppingListEditModel { Id = 5 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShoppingListPath}/makeShoppingList")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ShoppingListCreateModel>(body, JsonOptions);
                    return deserialized is not null; // could be stricter if needed
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedEdit, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.MakeShoppingListAsync(createModel);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(expectedEdit.Id));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void MakeShoppingListAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var createModel = new ShoppingListCreateModel();

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShoppingListPath}/makeShoppingList")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.MakeShoppingListAsync(createModel));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- AddAsync ----------
        [Test]
        public async Task AddAsync_PostsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new ShoppingListEditModel { Id = 1 };
            var expectedResponse = new CommandResponse { Succeeded = true, Message = "ok" };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShoppingListPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ShoppingListEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.AddAsync(model);

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
        public void AddAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var model = new ShoppingListEditModel { Id = 1 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Post, $"{BaseAddress}{ShoppingListPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.AddAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- UpdateAsync ----------
        [Test]
        public async Task UpdateAsync_PutsModel_AndReturnsCommandResponse()
        {
            // Arrange
            var model = new ShoppingListEditModel { Id = 2 };
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{ShoppingListPath}")
                .With(m =>
                {
                    var body = m.Content!.ReadAsStringAsync().Result;
                    var deserialized = JsonSerializer.Deserialize<ShoppingListEditModel>(body, JsonOptions);
                    return deserialized is not null && deserialized.Id == model.Id;
                })
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.UpdateAsync(model);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void UpdateAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var model = new ShoppingListEditModel { Id = 2 };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Put, $"{BaseAddress}{ShoppingListPath}")
                .Respond(HttpStatusCode.BadRequest);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.UpdateAsync(model));
            mockHttp.VerifyNoOutstandingExpectation();
        }

        // ---------- DeleteAsync ----------
        [Test]
        public async Task DeleteAsync_SendsDeleteWithId_AndReturnsCommandResponse()
        {
            // Arrange
            var id = 7;
            var expectedResponse = new CommandResponse { Succeeded = true };

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ShoppingListPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond("application/json", JsonSerializer.Serialize(expectedResponse, JsonOptions));

            var service = CreateService(mockHttp);

            // Act
            var result = await service.DeleteAsync(id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void DeleteAsync_Throws_OnNonSuccessStatusCode()
        {
            // Arrange
            var id = 7;

            var mockHttp = new MockHttpMessageHandler();

            mockHttp
                .Expect(HttpMethod.Delete, $"{BaseAddress}{ShoppingListPath}*")
                .With(m => m.RequestUri!.Query.Contains($"id={id}"))
                .Respond(HttpStatusCode.NotFound);

            var service = CreateService(mockHttp);

            // Act & Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await service.DeleteAsync(id));
            mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}