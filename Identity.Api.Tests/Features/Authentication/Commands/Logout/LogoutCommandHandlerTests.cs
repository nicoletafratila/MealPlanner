using Common.Data.DataContext;
using Identity.Api.Features.Authentication;
using Identity.Api.Features.Authentication.Commands.Logout;
using Identity.Data.Entities;
using Identity.Data.TableConfigurations;
using MealPlanner.Data.TableConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using RecipeBook.Data.TableConfigurations;

namespace Identity.Api.Tests.Features.Authentication.Commands.Logout
{
    [TestFixture]
    public class LogoutCommandHandlerTests
    {
        private Mock<SignInManager<Data.Entities.ApplicationUser>> _signInManagerMock = null!;
        private MealPlannerDbContext _dbContext = null!;
        private LogoutCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            // Minimal setup for SignInManager
            var userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<Data.Entities.ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<Data.Entities.ApplicationUser>>(),
                null, null, null, null);

            var tableConfigurationAssemblies = new TableConfigurationAssemblies([
                typeof(RecipeTableConfiguration).Assembly,
                typeof(MealPlanTableConfiguration).Assembly,
                typeof(RefreshTokenTableConfiguration).Assembly
            ]);
            _dbContext = new MealPlannerDbContext(
                new DbContextOptionsBuilder<MealPlannerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
                tableConfigurationAssemblies);

            _handler = new LogoutCommandHandler(_signInManagerMock.Object, _dbContext);
        }

        [TearDown]
        public void TearDown() => _dbContext.Dispose();

        [Test]
        public async Task Handle_CallsSignOut_AndReturnsSuccess()
        {
            // Arrange
            var command = new LogoutCommand();

            _signInManagerMock
                .Setup(s => s.SignOutAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
        }

        [Test]
        public async Task Handle_WithRefreshToken_RevokesMatchingToken()
        {
            // Arrange
            var (entity, rawToken) = RefreshTokenGenerator.CreateEntity("user-1", TimeSpan.FromDays(30));
            _dbContext.RefreshTokens.Add(entity);
            await _dbContext.SaveChangesAsync();

            var command = new LogoutCommand { RefreshToken = rawToken };
            _signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result!.Succeeded, Is.True);
            var stored = await _dbContext.RefreshTokens.SingleAsync(t => t.Id == entity.Id);
            Assert.That(stored.RevokedAtUtc, Is.Not.Null);
        }

        [Test]
        public async Task Handle_WithUnknownRefreshToken_StillReturnsSuccess()
        {
            // Arrange
            var command = new LogoutCommand { RefreshToken = "does-not-exist" };
            _signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result!.Succeeded, Is.True);
        }
    }
}
