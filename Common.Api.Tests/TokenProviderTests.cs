using Blazored.SessionStorage;
using Moq;

namespace Common.Api.Tests
{
    [TestFixture]
    public class TokenProviderTests
    {
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private TokenProvider _sut = null!;

        private const string TokenKey = Constants.MealPlanner.AuthToken;

        [SetUp]
        public void SetUp()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _sut = new TokenProvider(_sessionStorageMock.Object);
        }

        [Test]
        public void Ctor_NullSessionStorage_Throws()
        {
            Assert.That(
                () => new TokenProvider(null!),
                Throws.TypeOf<ArgumentNullException>()
                    .With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("sessionStorage"));
        }

        [Test]
        public async Task GetTokenAsync_DelegatesToSessionStorage_WithCorrectKey()
        {
            var expectedToken = "abc123";
            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(TokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var result = await _sut.GetTokenAsync(CancellationToken.None);

            Assert.That(result, Is.EqualTo(expectedToken));
            _sessionStorageMock.Verify(
                s => s.GetItemAsync<string?>(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void GetTokenAsync_CancelledToken_ThrowsOperationCanceledException_AndDoesNotCallStorage()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(
                async () => await _sut.GetTokenAsync(cts.Token),
                Throws.InstanceOf<OperationCanceledException>());

            _sessionStorageMock.VerifyNoMocks();
        }

        [Test]
        public void SetTokenAsync_NullToken_ThrowsArgumentNullException_AndDoesNotCallStorage()
        {
            Assert.That(
                async () => await _sut.SetTokenAsync(null!, CancellationToken.None),
                Throws.TypeOf<ArgumentNullException>());

            _sessionStorageMock.VerifyNoOtherCalls();
        }

        [Test]
        public void SetTokenAsync_EmptyToken_ThrowsArgumentException_AndDoesNotCallStorage()
        {
            Assert.That(
                async () => await _sut.SetTokenAsync(string.Empty, CancellationToken.None),
                Throws.TypeOf<ArgumentException>());

            _sessionStorageMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetTokenAsync_ValidToken_CallsSessionStorageWithCorrectKeyAndValue()
        {
            var token = "valid-token";
            _sessionStorageMock
                .Setup(s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.SetTokenAsync(token, CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void SetTokenAsync_CancelledToken_ThrowsOperationCanceledException_AndDoesNotCallStorage()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(
                async () => await _sut.SetTokenAsync("token", cts.Token),
                Throws.InstanceOf<OperationCanceledException>());

            _sessionStorageMock.VerifyNoMocks();
        }

        [Test]
        public async Task RemoveTokenAsync_CallsSessionStorageWithCorrectKey()
        {
            _sessionStorageMock
                .Setup(s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.RemoveTokenAsync(CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void RemoveTokenAsync_CancelledToken_ThrowsOperationCanceledException_AndDoesNotCallStorage()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(
                async () => await _sut.RemoveTokenAsync(cts.Token),
                Throws.InstanceOf<OperationCanceledException>());

            _sessionStorageMock.VerifyNoMocks();
        }
    }
}