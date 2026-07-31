using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Common.Http;
using MealPlanner.Services.Http;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Mobile.Services;
using MealPlanner.UI.Mobile.ViewModels;
using Moq;

namespace MealPlanner.UI.Mobile.Tests.ViewModels
{
    [TestFixture]
    public class AppShellViewModelTests
    {
        private Mock<IMealPlanService> _mealPlanServiceMock = null!;
        private Mock<ITokenProvider> _tokenProviderMock = null!;
        private AuthenticationStateService _authStateService = null!;
        private AppShellViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mealPlanServiceMock = new Mock<IMealPlanService>(MockBehavior.Strict);
            _tokenProviderMock = new Mock<ITokenProvider>(MockBehavior.Strict);
            _authStateService = new AuthenticationStateService(_tokenProviderMock.Object);
            _viewModel = new AppShellViewModel(_mealPlanServiceMock.Object, _authStateService);
        }

        private static string BuildJwt(Dictionary<string, object> payload)
        {
            var header = new Dictionary<string, object> { ["alg"] = "none", ["typ"] = "JWT" };
            var headerSegment = Base64UrlEncode(JsonSerializer.Serialize(header));
            var payloadSegment = Base64UrlEncode(JsonSerializer.Serialize(payload));
            return $"{headerSegment}.{payloadSegment}.signature";
        }

        private static string Base64UrlEncode(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        [Test]
        public async Task LoadCurrentAsync_MealPlanServiceSucceeds_SetsCurrentMealPlanAndHasCurrentMealPlanTrue()
        {
            var mealPlan = new MealPlanModel(Guid.NewGuid(), "This week");

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ReturnsAsync(mealPlan);
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.CurrentMealPlan, Is.SameAs(mealPlan));
                Assert.That(_viewModel.HasCurrentMealPlan, Is.True);
            }
        }

        [Test]
        public async Task LoadCurrentAsync_MealPlanServiceThrows_SetsCurrentMealPlanNull()
        {
            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ThrowsAsync(new HttpRequestException("boom"));
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_viewModel.CurrentMealPlan, Is.Null);
                Assert.That(_viewModel.HasCurrentMealPlan, Is.False);
            }
        }

        [Test]
        public async Task LoadCurrentAsync_TokenHasAdminRoleClaim_SetsIsAdminTrue()
        {
            var token = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                [ClaimTypes.Role] = "admin"
            });

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ReturnsAsync((MealPlanModel?)null);
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            Assert.That(_viewModel.IsAdmin, Is.True);
        }

        [Test]
        public async Task LoadCurrentAsync_TokenHasNonAdminRoleClaim_SetsIsAdminFalse()
        {
            var token = BuildJwt(new Dictionary<string, object>
            {
                ["sub"] = "user-1",
                [ClaimTypes.Role] = "member"
            });

            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ReturnsAsync((MealPlanModel?)null);
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            Assert.That(_viewModel.IsAdmin, Is.False);
        }

        [Test]
        public async Task LoadCurrentAsync_NoToken_SetsIsAdminFalse()
        {
            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ReturnsAsync((MealPlanModel?)null);
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            Assert.That(_viewModel.IsAdmin, Is.False);
        }

        [Test]
        public async Task LoadCurrentAsync_TokenProviderThrows_CatchesAndSetsIsAdminFalse()
        {
            _mealPlanServiceMock
                .Setup(s => s.GetCurrentAsync(CancellationToken.None))
                .ReturnsAsync((MealPlanModel?)null);
            _tokenProviderMock
                .Setup(t => t.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("token store unavailable"));

            await _viewModel.LoadCurrentCommand.ExecuteAsync(null);

            Assert.That(_viewModel.IsAdmin, Is.False);
        }
    }
}
