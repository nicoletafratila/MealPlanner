using Blazored.SessionStorage;
using MealPlanner.UI.Web.Services.Identities;
using Moq;

namespace MealPlanner.UI.Web.Tests.Services.Identities
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

            var dto = new TestDto { Name = "A", Value = 1 };

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
            var expectedKey = typeof(TestDto).FullName!;

            storageMock
                .Setup(s => s.SetItemAsync(expectedKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            var dto = new TestDto { Name = "B", Value = 2 };

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

            var json = "{\"Name\":\"C\",\"Value\":3}";
            storageMock
                .Setup(s => s.GetItemAsync<string?>(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);

            // Act
            var result = await SessionExtensions.GetItemAsync<TestDto>(storageMock.Object, key);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Name, Is.EqualTo("C"));
                Assert.That(result.Value, Is.EqualTo(3));
            });
        }

        [Test]
        public async Task GetItemAsync_UsesTypeFullName_WhenNameIsNullOrWhitespace()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            var expectedKey = typeof(TestDto).FullName!;

            var json = "{\"Name\":\"D\",\"Value\":4}";
            storageMock
                .Setup(s => s.GetItemAsync<string?>(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(json);

            // Act
            var result = await SessionExtensions.GetItemAsync<TestDto>(storageMock.Object, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Name, Is.EqualTo("D"));
                Assert.That(result.Value, Is.EqualTo(4));
            });
        }

        [Test]
        public async Task GetItemAsync_ReturnsDefault_WhenStoredValueIsNullOrEmpty()
        {
            // Arrange
            var storageMock = new Mock<ISessionStorageService>();
            var key = typeof(TestDto).FullName!;

            storageMock
                .Setup(s => s.GetItemAsync<string?>(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await SessionExtensions.GetItemAsync<TestDto>(storageMock.Object);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SetItemAsync_ThrowsArgumentNullException_WhenSessionStorageIsNull()
        {
            // Arrange
            ISessionStorageService? storage = null;
            var dto = new TestDto { Name = "E", Value = 5 };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SessionExtensions.SetItemAsync(storage!, dto));
        }

        [Test]
        public void GetItemAsync_ThrowsArgumentNullException_WhenSessionStorageIsNull()
        {
            // Arrange
            ISessionStorageService? storage = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await SessionExtensions.GetItemAsync<TestDto>(storage!));
        }
    }
}