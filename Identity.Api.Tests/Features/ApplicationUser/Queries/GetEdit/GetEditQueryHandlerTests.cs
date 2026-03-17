using AutoMapper;
using Identity.Api.Features.ApplicationUser.Queries.GetEdit;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryHandlerTests
    {
        private Mock<UserManager<Common.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private GetEditQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Common.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            _handler = new GetEditQueryHandler(_userManagerMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Ctor_NullUserManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(null!, _mapperMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetEditQueryHandler(_userManagerMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsEmptyModel()
        {
            // Arrange
            var query = new GetEditQuery("alice");

            _userManagerMock
                .Setup(m => m.FindByNameAsync("alice"))
                .ReturnsAsync((Common.Data.Entities.ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.EmailAddress, Is.Null.Or.Empty);
                Assert.That(result.Username, Is.Null.Or.Empty);
            });

            _userManagerMock.Verify(m => m.FindByNameAsync("alice"), Times.Once);
            _mapperMock.Verify(m => m.Map<ApplicationUserEditModel>(It.IsAny<Common.Data.Entities.ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task Handle_UserFound_ReturnsMappedModel()
        {
            // Arrange
            var user = new Common.Data.Entities.ApplicationUser
            {
                Id = "1",
                UserName = "alice",
                Email = "alice@example.com"
            };

            var mapped = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "alice",
                EmailAddress = "alice@example.com",
                IsActive = true
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("alice"))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(m => m.Map<ApplicationUserEditModel>(user))
                .Returns(mapped);

            var query = new GetEditQuery("alice");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.UserId, Is.EqualTo("1"));
                Assert.That(result.Username, Is.EqualTo("alice"));
                Assert.That(result.EmailAddress, Is.EqualTo("alice@example.com"));
            });

            _userManagerMock.Verify(m => m.FindByNameAsync("alice"), Times.Once);
            _mapperMock.Verify(m => m.Map<ApplicationUserEditModel>(user), Times.Once);
        }

        [Test]
        public async Task Handle_MapperReturnsNull_FallsBackToEmptyModel()
        {
            // Arrange
            var user = new Common.Data.Entities.ApplicationUser
            {
                Id = "1",
                UserName = "alice"
            };

            _userManagerMock
                .Setup(m => m.FindByNameAsync("alice"))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(m => m.Map<ApplicationUserEditModel?>(user))
                .Returns((ApplicationUserEditModel?)null);

            var query = new GetEditQuery("alice");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Username, Is.Null.Or.Empty);

            _userManagerMock.Verify(m => m.FindByNameAsync("alice"), Times.Once);
            _mapperMock.Verify(m => m.Map<ApplicationUserEditModel>(user), Times.Once);
        }
    }
}