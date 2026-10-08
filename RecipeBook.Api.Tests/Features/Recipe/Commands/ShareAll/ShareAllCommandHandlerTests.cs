using Common.Models;
using Common.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.Share;
using RecipeBook.Api.Features.Recipe.Commands.ShareAll;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.ShareAll
{
    [TestFixture]
    public class ShareAllCommandHandlerTests
    {
        private Mock<IRecipeRepository> _repoMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ISender> _mediatorMock = null!;
        private Mock<ILogger<ShareAllCommandHandler>> _loggerMock = null!;
        private ShareAllCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IRecipeRepository>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _mediatorMock = new Mock<ISender>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<ShareAllCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new ShareAllCommandHandler(
                _repoMock.Object,
                _currentUserMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(null!, _currentUserMock.Object, _mediatorMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, null!, _mediatorMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMediator_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _currentUserMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new ShareAllCommandHandler(_repoMock.Object, _currentUserMock.Object, _mediatorMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_NoCurrentUser_ReturnsFailedResponse_AndDoesNotQueryRecipes()
        {
            _currentUserMock.Setup(s => s.UserId).Returns((string?)null);
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User id is required."));
            }

            _repoMock.Verify(r => r.GetAllByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_NoRecipes_ReturnsSuccess_AndDoesNotSendShareCommand()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe>());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _mediatorMock.Verify(m => m.Send(It.IsAny<ShareCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_MultipleRecipes_SharesEachViaShareCommand_AndReturnsSuccess()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe1 = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "R1", UserId = "user1" };
            var recipe2 = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "R2", UserId = "user1" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe1, recipe2 });

            _mediatorMock
                .Setup(m => m.Send(It.Is<ShareCommand>(c => c.RecipeId == recipe1.Id && c.TargetUserId == "user2"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CommandResponse.Success());

            _mediatorMock
                .Setup(m => m.Send(It.Is<ShareCommand>(c => c.RecipeId == recipe2.Id && c.TargetUserId == "user2"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CommandResponse.Success());

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _mediatorMock.Verify(m => m.Send(It.IsAny<ShareCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task Handle_ShareCommandFails_ReturnsFailedResponse_AndStopsSharingRemaining()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe1 = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "R1", UserId = "user1" };
            var recipe2 = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "R2", UserId = "user1" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe1, recipe2 });

            _mediatorMock
                .Setup(m => m.Send(It.Is<ShareCommand>(c => c.RecipeId == recipe1.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CommandResponse.Failed("Name clash could not be resolved"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Name clash could not be resolved"));
            }

            _mediatorMock.Verify(m => m.Send(It.Is<ShareCommand>(c => c.RecipeId == recipe2.Id), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Handle_ShareCommandReturnsNull_ReturnsFailedResponse()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            var recipe = new Data.Entities.Recipe { Id = Guid.NewGuid(), Name = "R1", UserId = "user1" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Data.Entities.Recipe> { recipe });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ShareCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CommandResponse?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }
        }

        [Test]
        public async Task Handle_ExceptionDuringProcessing_LogsError_AndReturnsFailedResponse()
        {
            var command = new ShareAllCommand { TargetUserId = "user2" };

            _repoMock
                .Setup(r => r.GetAllByUserAsync("user1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
