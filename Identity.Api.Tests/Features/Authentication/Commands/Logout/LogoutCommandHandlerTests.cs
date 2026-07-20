using Identity.Api.Features.Authentication.Commands.Logout;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.Logout
{
    [TestFixture]
    public class LogoutCommandHandlerTests
    {
        private Mock<SignInManager<Data.Entities.ApplicationUser>> _signInManagerMock = null!;
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

            _handler = new LogoutCommandHandler(_signInManagerMock.Object);
        }

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
    }
}