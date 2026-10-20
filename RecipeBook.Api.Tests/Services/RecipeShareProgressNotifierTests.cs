using Microsoft.AspNetCore.SignalR;
using Moq;
using RecipeBook.Api.Hubs;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class RecipeShareProgressNotifierTests
    {
        private Mock<IHubContext<RecipeShareProgressHub>> _hubContextMock = null!;
        private Mock<IHubClients> _hubClientsMock = null!;
        private Mock<IClientProxy> _clientProxyMock = null!;
        private RecipeShareProgressNotifier _notifier = null!;

        [SetUp]
        public void SetUp()
        {
            _hubContextMock = new Mock<IHubContext<RecipeShareProgressHub>>(MockBehavior.Strict);
            _hubClientsMock = new Mock<IHubClients>(MockBehavior.Strict);
            _clientProxyMock = new Mock<IClientProxy>(MockBehavior.Strict);

            _hubContextMock.Setup(h => h.Clients).Returns(_hubClientsMock.Object);

            _notifier = new RecipeShareProgressNotifier(_hubContextMock.Object);
        }

        [Test]
        public void Ctor_NullHubContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new RecipeShareProgressNotifier(null!));
        }

        [Test]
        public async Task NotifyAsync_NullJob_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _notifier.NotifyAsync(null!, CancellationToken.None));
        }

        [Test]
        public async Task NotifyAsync_SendsStatusToJobGroup()
        {
            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Id = jobId,
                Status = JobStatus.Running,
                TotalCount = 10,
                ProcessedCount = 5,
                ErrorMessage = null
            };

            _hubClientsMock.Setup(c => c.Group(jobId.ToString())).Returns(_clientProxyMock.Object);

            object[]? sentArgs = null;
            _clientProxyMock
                .Setup(c => c.SendCoreAsync("ShareProgress", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((_, args, _) => sentArgs = args)
                .Returns(Task.CompletedTask);

            await _notifier.NotifyAsync(job, CancellationToken.None);

            Assert.That(sentArgs, Is.Not.Null);
            var status = (Shared.Models.JobStatusModel)sentArgs![0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(status.JobId, Is.EqualTo(jobId));
                Assert.That(status.Status, Is.EqualTo(Shared.Enums.JobStatus.Running));
                Assert.That(status.PercentComplete, Is.EqualTo(50));
            }

            _hubClientsMock.Verify(c => c.Group(jobId.ToString()), Times.Once);
        }

        [Test]
        public async Task NotifyAsync_TotalCountZero_PercentCompleteIsZero()
        {
            var jobId = Guid.NewGuid();
            var job = new Job { Id = jobId, TotalCount = 0, ProcessedCount = 0 };

            _hubClientsMock.Setup(c => c.Group(jobId.ToString())).Returns(_clientProxyMock.Object);

            object[]? sentArgs = null;
            _clientProxyMock
                .Setup(c => c.SendCoreAsync("ShareProgress", It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((_, args, _) => sentArgs = args)
                .Returns(Task.CompletedTask);

            await _notifier.NotifyAsync(job, CancellationToken.None);

            var status = (Shared.Models.JobStatusModel)sentArgs![0];
            Assert.That(status.PercentComplete, Is.Zero);
        }
    }
}
