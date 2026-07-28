using Common.Data.DataContext;
using Common.Models;
using Identity.Api.Features.Authentication;
using Identity.Api.Features.Authentication.Commands.RefreshToken;
using Identity.Data.TableConfigurations;
using Identity.Shared.Models;
using MealPlanner.Data.TableConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Data.TableConfigurations;

namespace Identity.Api.Tests.Features.Authentication.Commands.RefreshToken
{
    [TestFixture]
    public class RefreshTokenCommandHandlerTests
    {
        private Mock<UserManager<Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock = null!;
        private MealPlannerDbContext _dbContext = null!;
        private RefreshTokenCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>(MockBehavior.Loose);

            var tableConfigurationAssemblies = new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly,
                typeof(RefreshTokenTableConfiguration).Assembly
            ]);
            _dbContext = new MealPlannerDbContext(
                new DbContextOptionsBuilder<MealPlannerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                tableConfigurationAssemblies);

            _handler = new RefreshTokenCommandHandler(_userManagerMock.Object, _dbContext, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown() => _dbContext.Dispose();

        [Test]
        public async Task Handle_UnknownToken_ReturnsInvalidRefreshToken()
        {
            var command = new RefreshTokenCommand { Model = new RefreshTokenModel { RefreshToken = "unknown" } };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid refresh token."));
        }

        [Test]
        public async Task Handle_RevokedToken_ReturnsInvalidRefreshToken()
        {
            var (entity, rawToken) = RefreshTokenGenerator.CreateEntity("user-1", TimeSpan.FromDays(30));
            entity.RevokedAtUtc = DateTime.UtcNow;
            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync();

            var command = new RefreshTokenCommand { Model = new RefreshTokenModel { RefreshToken = rawToken } };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid refresh token."));
        }

        [Test]
        public async Task Handle_ExpiredToken_ReturnsRefreshTokenExpired()
        {
            var (entity, rawToken) = RefreshTokenGenerator.CreateEntity("user-1", TimeSpan.FromDays(-1));
            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync();

            var command = new RefreshTokenCommand { Model = new RefreshTokenModel { RefreshToken = rawToken } };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Refresh token has expired."));
        }

        [Test]
        public async Task Handle_InactiveUser_ReturnsInvalidRefreshToken()
        {
            var (entity, rawToken) = RefreshTokenGenerator.CreateEntity("user-1", TimeSpan.FromDays(30));
            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync();

            var user = new Data.Entities.ApplicationUser { Id = "user-1", UserName = "user", IsActive = false };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var command = new RefreshTokenCommand { Model = new RefreshTokenModel { RefreshToken = rawToken } };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.False);
            Assert.That(result.Message, Is.EqualTo("Invalid refresh token."));
        }

        [Test]
        public async Task Handle_ValidToken_RotatesAndReturnsNewJwtAndRefreshToken()
        {
            var (entity, rawToken) = RefreshTokenGenerator.CreateEntity("user-1", TimeSpan.FromDays(30));
            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync();

            var user = new Data.Entities.ApplicationUser { Id = "user-1", UserName = "user", IsActive = true };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["admin"]);

            var command = new RefreshTokenCommand { Model = new RefreshTokenModel { RefreshToken = rawToken } };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.TypeOf<LoginCommandResponse>());
            var response = (LoginCommandResponse)result!;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(response.Succeeded, Is.True);
                Assert.That(response.JwtBearer, Is.Not.Null.And.Not.Empty);
                Assert.That(response.RefreshToken, Is.Not.Null.And.Not.Empty);
                Assert.That(response.RefreshToken, Is.Not.EqualTo(rawToken));
            }

            var oldToken = await _dbContext.RefreshTokens.SingleAsync(t => t.Id == entity.Id);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(oldToken.RevokedAtUtc, Is.Not.Null);
                Assert.That(oldToken.ReplacedByTokenId, Is.Not.Null);
            }

            var allTokens = await _dbContext.RefreshTokens.ToListAsync();
            Assert.That(allTokens, Has.Count.EqualTo(2));
        }
    }
}
