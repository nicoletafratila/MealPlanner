using System.Security.Claims;using Microsoft.AspNetCore.Http; using Moq;

namespace Common.Services.Tests
{
    [TestFixture]
    public class CurrentUserServiceTests
    {
        private Mock<IHttpContextAccessor> _accessorMock = null!;
        private CurrentUserService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _accessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            _sut = new CurrentUserService(_accessorMock.Object);
        }

        [Test]
        public void UserId_ReturnsNameIdentifierClaim_WhenUserIsAuthenticated()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-42") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = principal };
            _accessorMock.Setup(a => a.HttpContext).Returns(context);

            Assert.That(_sut.UserId, Is.EqualTo("user-42"));
        }

        [Test]
        public void UserId_ReturnsNull_WhenHttpContextIsNull()
        {
            _accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            Assert.That(_sut.UserId, Is.Null);
        }

        [Test]
        public void UserId_ReturnsNull_WhenNameIdentifierClaimMissing()
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "alice") };
            var identity = new ClaimsIdentity(claims, "Test");
            var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
            _accessorMock.Setup(a => a.HttpContext).Returns(context);

            Assert.That(_sut.UserId, Is.Null);
        }

        [Test]
        public void UserId_ReturnsNull_WhenUserHasNoIdentity()
        {
            var context = new DefaultHttpContext { User = new ClaimsPrincipal() };
            _accessorMock.Setup(a => a.HttpContext).Returns(context);

            Assert.That(_sut.UserId, Is.Null);
        }

        [Test]
        public void UserId_ReturnsCorrectValue_WhenMultipleClaimsPresent()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "alice"),
                new Claim(ClaimTypes.Email, "alice@example.com"),
                new Claim(ClaimTypes.NameIdentifier, "user-99")
            };
            var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) };
            _accessorMock.Setup(a => a.HttpContext).Returns(context);

            Assert.That(_sut.UserId, Is.EqualTo("user-99"));
        }
    }
}
