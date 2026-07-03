using Blazored.SessionStorage;
using MealPlanner.Shared.Models;
using MealPlanner.UI.Web.Services;
using Moq;

namespace MealPlanner.UI.Web.Tests.Services
{
    [TestFixture]
    public class SessionExtensionsTests
    {
        [Test]
        public async Task SetItemAsync_UsesExplicitName_AsKey()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            const string key = "custom-key";

            storageMock
                .Setup(s => s.SetItemAsync(key, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            var dto = new ShopModel { Name = "A", Id = Guid.NewGuid() };

            // Act
            await storageMock.Object.SetItemAsync(dto, key);

            // Assert
            storageMock.Verify(
                s => s.SetItemAsync(key, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SetItemAsync_UsesTypeFullName_WhenNameIsNullOrWhitespace()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            var expectedKey = typeof(ShopModel).FullName!;

            storageMock
                .Setup(s => s.SetItemAsync(expectedKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            var dto = new ShopModel { Name = "B", Id = Guid.NewGuid() };

            // Act
            await storageMock.Object.SetItemAsync(dto, null);

            // Assert
            storageMock.Verify(
                s => s.SetItemAsync(expectedKey, It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetItemAsync_UsesExplicitName_AsKey_AndDeserializes()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            const string key = "explicit-key";

            var id = Guid.NewGuid();
            var json = $"{{\"Name\":\"C\",\"Id\":\"{id}\"}}";
            storageMock
                .Setup(s => s.GetItemAsync<string?>(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);

            // Act
            var result = await SessionExtensions.GetItemAsync<ShopModel>(storageMock.Object, key);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Name, Is.EqualTo("C"));
                Assert.That(result.Id, Is.EqualTo(id));
            }
        }

        [Test]
        public async Task GetItemAsync_UsesTypeFullName_WhenNameIsNullOrWhitespace()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            var expectedKey = typeof(ShopModel).FullName!;

            var id = Guid.NewGuid();
            var json = $"{{\"Name\":\"D\",\"Id\":\"{id}\"}}";
            storageMock
                .Setup(s => s.GetItemAsync<string?>(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);

            // Act
            var result = await SessionExtensions.GetItemAsync<ShopModel>(storageMock.Object, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Name, Is.EqualTo("D"));
                Assert.That(result.Id, Is.EqualTo(id));
            }
        }

        [Test]
        public async Task GetItemAsync_ReturnsDefault_WhenStoredValueIsNullOrEmpty()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            var key = typeof(ShopModel).FullName!;

            storageMock
                .Setup(s => s.GetItemAsync<string?>(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await SessionExtensions.GetItemAsync<ShopModel>(storageMock.Object);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SetItemAsync_ThrowsArgumentNullException_WhenSessionStorageIsNull()
        {
            // Arrange
            ISessionStorageService? storage = null;
            var dto = new ShopModel { Name = "E", Id = Guid.NewGuid() };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SessionExtensions.SetItemAsync(storage!, dto));
        }

        [Test]
        public void GetItemAsync_ThrowsArgumentNullException_WhenSessionStorageIsNull()
        {
            // Arrange
            ISessionStorageService? storage = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SessionExtensions.GetItemAsync<ShopModel>(storage!));
        }
    }
}
