using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Moq;

namespace Common.Core.Tests
{
    [TestFixture]
    public class TokenProviderTests
    {
        private Mock<ISessionStorageService> _sessionStorageMock = null!;
        private Mock<ILocalStorageService> _localStorageMock = null!;
        private TokenProvider _sut = null!;

        private const string TokenKey = Constants.MealPlanner.AuthToken;
        private const string RefreshTokenKey = Constants.MealPlanner.RefreshToken;

        [SetUp]
        public void SetUp()
        {
            _sessionStorageMock = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _localStorageMock = new Mock<ILocalStorageService>(MockBehavior.Strict);
            _sut = new TokenProvider(_sessionStorageMock.Object, _localStorageMock.Object);
        }

        [Test]
        public void Ctor_NullSessionStorage_Throws()
        {
            Assert.That(
                () => new TokenProvider(null!, _localStorageMock.Object),
                Throws.TypeOf<ArgumentNullException>()
                    .With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("sessionStorage"));
        }

        [Test]
        public void Ctor_NullLocalStorage_Throws()
        {
            Assert.That(
                () => new TokenProvider(_sessionStorageMock.Object, null!),
                Throws.TypeOf<ArgumentNullException>()
                    .With.Property(nameof(ArgumentNullException.ParamName))
                    .EqualTo("localStorage"));
        }

        [Test]
        public async Task GetTokenAsync_PrefersLocalStorage_WhenPresent()
        {
            var expectedToken = "abc123";
            _localStorageMock
                .Setup(s => s.GetItemAsync<string?>(TokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var result = await _sut.GetTokenAsync(CancellationToken.None);

            Assert.That(result, Is.EqualTo(expectedToken));
            _localStorageMock.Verify(
                s => s.GetItemAsync<string?>(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetTokenAsync_FallsBackToSessionStorage_WhenLocalStorageEmpty()
        {
            var expectedToken = "abc123";
            _localStorageMock
                .Setup(s => s.GetItemAsync<string?>(TokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
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
            _localStorageMock.VerifyNoMocks();
        }

        [Test]
        public void SetTokenAsync_NullToken_ThrowsArgumentNullException_AndDoesNotCallStorage()
        {
            Assert.That(
                async () => await _sut.SetTokenAsync(null!, cancellationToken: CancellationToken.None),
                Throws.TypeOf<ArgumentNullException>());

            _sessionStorageMock.VerifyNoOtherCalls();
            _localStorageMock.VerifyNoOtherCalls();
        }

        [Test]
        public void SetTokenAsync_EmptyToken_ThrowsArgumentException_AndDoesNotCallStorage()
        {
            Assert.That(
                async () => await _sut.SetTokenAsync(string.Empty, cancellationToken: CancellationToken.None),
                Throws.TypeOf<ArgumentException>());

            _sessionStorageMock.VerifyNoOtherCalls();
            _localStorageMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetTokenAsync_NotPersistent_WritesSessionStorage_AndClearsLocalStorage()
        {
            var token = "valid-token";
            _sessionStorageMock
                .Setup(s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _localStorageMock
                .Setup(s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.SetTokenAsync(token, persistent: false, cancellationToken: CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()),
                Times.Once);
            _localStorageMock.Verify(
                s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SetTokenAsync_Persistent_WritesLocalStorage_AndClearsSessionStorage()
        {
            var token = "valid-token";
            _localStorageMock
                .Setup(s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _sessionStorageMock
                .Setup(s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.SetTokenAsync(token, persistent: true, cancellationToken: CancellationToken.None);

            _localStorageMock.Verify(
                s => s.SetItemAsync(TokenKey, token, It.IsAny<CancellationToken>()),
                Times.Once);
            _sessionStorageMock.Verify(
                s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void SetTokenAsync_CancelledToken_ThrowsOperationCanceledException_AndDoesNotCallStorage()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(
                async () => await _sut.SetTokenAsync("token", cancellationToken: cts.Token),
                Throws.InstanceOf<OperationCanceledException>());

            _sessionStorageMock.VerifyNoMocks();
            _localStorageMock.VerifyNoMocks();
        }

        [Test]
        public async Task RemoveTokenAsync_CallsBothStorages_WithCorrectKey()
        {
            _sessionStorageMock
                .Setup(s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _localStorageMock
                .Setup(s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.RemoveTokenAsync(CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.RemoveItemAsync(TokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
            _localStorageMock.Verify(
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
            _localStorageMock.VerifyNoMocks();
        }

        [Test]
        public async Task GetRefreshTokenAsync_PrefersLocalStorage_WhenPresent()
        {
            var expectedToken = "refresh-abc123";
            _localStorageMock
                .Setup(s => s.GetItemAsync<string?>(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var result = await _sut.GetRefreshTokenAsync(CancellationToken.None);

            Assert.That(result, Is.EqualTo(expectedToken));
            _localStorageMock.Verify(
                s => s.GetItemAsync<string?>(RefreshTokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetRefreshTokenAsync_FallsBackToSessionStorage_WhenLocalStorageEmpty()
        {
            var expectedToken = "refresh-abc123";
            _localStorageMock
                .Setup(s => s.GetItemAsync<string?>(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);
            _sessionStorageMock
                .Setup(s => s.GetItemAsync<string?>(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            var result = await _sut.GetRefreshTokenAsync(CancellationToken.None);

            Assert.That(result, Is.EqualTo(expectedToken));
        }

        [Test]
        public void SetRefreshTokenAsync_EmptyToken_ThrowsArgumentException_AndDoesNotCallStorage()
        {
            Assert.That(
                async () => await _sut.SetRefreshTokenAsync(string.Empty, cancellationToken: CancellationToken.None),
                Throws.TypeOf<ArgumentException>());

            _sessionStorageMock.VerifyNoOtherCalls();
            _localStorageMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetRefreshTokenAsync_NotPersistent_WritesSessionStorage_AndClearsLocalStorage()
        {
            var token = "refresh-token";
            _sessionStorageMock
                .Setup(s => s.SetItemAsync(RefreshTokenKey, token, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _localStorageMock
                .Setup(s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.SetRefreshTokenAsync(token, persistent: false, cancellationToken: CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.SetItemAsync(RefreshTokenKey, token, It.IsAny<CancellationToken>()),
                Times.Once);
            _localStorageMock.Verify(
                s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SetRefreshTokenAsync_Persistent_WritesLocalStorage_AndClearsSessionStorage()
        {
            var token = "refresh-token";
            _localStorageMock
                .Setup(s => s.SetItemAsync(RefreshTokenKey, token, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _sessionStorageMock
                .Setup(s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.SetRefreshTokenAsync(token, persistent: true, cancellationToken: CancellationToken.None);

            _localStorageMock.Verify(
                s => s.SetItemAsync(RefreshTokenKey, token, It.IsAny<CancellationToken>()),
                Times.Once);
            _sessionStorageMock.Verify(
                s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RemoveRefreshTokenAsync_CallsBothStorages_WithCorrectKey()
        {
            _sessionStorageMock
                .Setup(s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            _localStorageMock
                .Setup(s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            await _sut.RemoveRefreshTokenAsync(CancellationToken.None);

            _sessionStorageMock.Verify(
                s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
            _localStorageMock.Verify(
                s => s.RemoveItemAsync(RefreshTokenKey, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
