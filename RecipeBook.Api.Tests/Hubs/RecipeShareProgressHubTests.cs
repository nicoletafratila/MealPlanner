using Common.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using RecipeBook.Api.Hubs;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities;

namespace RecipeBook.Api.Tests.Hubs
{
    [TestFixture]
    public class RecipeShareProgressHubTests
    {
        private Mock<IJobRepository> _jobRepoMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<IGroupManager> _groupsMock = null!;
        private Mock<HubCallerContext> _contextMock = null!;
        private RecipeShareProgressHub _hub = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _groupsMock = new Mock<IGroupManager>(MockBehavior.Strict);
            _contextMock = new Mock<HubCallerContext>(MockBehavior.Loose);

            _contextMock.Setup(c => c.ConnectionId).Returns("connection1");
            _contextMock.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);

            _hub = new RecipeShareProgressHub(_jobRepoMock.Object, _currentUserMock.Object)
            {
                Context = _contextMock.Object,
                Groups = _groupsMock.Object
            };
        }

        [TearDown]
        public void TearDown() => _hub.Dispose();

        [Test]
        public void JoinJobAsync_JobNotFound_ThrowsHubException()
        {
            var jobId = Guid.NewGuid();
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync((Job?)null);

            Assert.ThrowsAsync<HubException>(async () => await _hub.JoinJobAsync(jobId));
        }

        [Test]
        public void JoinJobAsync_JobOwnedByDifferentUser_ThrowsHubException()
        {
            var jobId = Guid.NewGuid();
            var job = new Job { Id = jobId, OwnerUserId = "someoneElse" };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);
            _currentUserMock.Setup(c => c.UserId).Returns("user1");

            Assert.ThrowsAsync<HubException>(async () => await _hub.JoinJobAsync(jobId));
        }

        [Test]
        public async Task JoinJobAsync_OwnedJob_AddsConnectionToJobGroup()
        {
            var jobId = Guid.NewGuid();
            var job = new Job { Id = jobId, OwnerUserId = "user1" };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);
            _currentUserMock.Setup(c => c.UserId).Returns("user1");
            _groupsMock
                .Setup(g => g.AddToGroupAsync("connection1", jobId.ToString(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _hub.JoinJobAsync(jobId);

            _groupsMock.Verify(
                g => g.AddToGroupAsync("connection1", jobId.ToString(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
