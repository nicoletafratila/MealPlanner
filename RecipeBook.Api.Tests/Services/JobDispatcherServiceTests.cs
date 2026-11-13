using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Tests.Services
{
    [TestFixture]
    public class JobDispatcherServiceTests
    {
        private Mock<IJobRepository> _jobRepoMock = null!;
        private Mock<IRecipeShareProgressNotifier> _notifierMock = null!;
        private Mock<IRecipeJobProcessor> _processorMock = null!;
        private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
        private Mock<ILogger<JobDispatcherService>> _loggerMock = null!;
        private JobQueue _queue = null!;
        private JobDispatcherService _dispatcher = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>(MockBehavior.Strict);
            _notifierMock = new Mock<IRecipeShareProgressNotifier>(MockBehavior.Strict);
            _processorMock = new Mock<IRecipeJobProcessor>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<JobDispatcherService>>(MockBehavior.Loose);
            _queue = new JobQueue();

            _scopeFactoryMock = new Mock<IServiceScopeFactory>(MockBehavior.Strict);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(CreateScope);

            _dispatcher = new JobDispatcherService(_scopeFactoryMock.Object, _queue, _loggerMock.Object);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dispatcher.StopAsync(CancellationToken.None);
            _dispatcher.Dispose();
        }

        private IServiceScope CreateScope()
        {
            var providerMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            providerMock.Setup(p => p.GetService(typeof(IJobRepository))).Returns(_jobRepoMock.Object);
            providerMock.Setup(p => p.GetService(typeof(IRecipeShareProgressNotifier))).Returns(_notifierMock.Object);
            providerMock.Setup(p => p.GetService(typeof(IRecipeJobProcessor))).Returns(_processorMock.Object);

            var scopeMock = new Mock<IServiceScope>(MockBehavior.Strict);
            scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
            scopeMock.Setup(s => s.Dispose());

            return scopeMock.Object;
        }

        [Test]
        public async Task StartAsync_OrphanedRunningJob_MarksFailedAndNotifies()
        {
            var jobId = Guid.NewGuid();
            var orphanedJob = new Job { Id = jobId, Status = JobStatus.Running };

            _jobRepoMock.Setup(r => r.GetOrphanedRunningAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Job> { orphanedJob });
            _jobRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Job>());

            Job? updatedJob = null;
            _jobRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback<Job, CancellationToken>((j, _) => updatedJob = j)
                .Returns(Task.CompletedTask);

            var notifiedTcs = new TaskCompletionSource();
            _notifierMock.Setup(n => n.NotifyAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
                .Callback(() => notifiedTcs.TrySetResult())
                .Returns(Task.CompletedTask);

            await _dispatcher.StartAsync(CancellationToken.None);
            await Task.WhenAny(notifiedTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

            Assert.That(updatedJob, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(updatedJob!.Status, Is.EqualTo(JobStatus.Failed));
                Assert.That(updatedJob.ErrorMessage, Is.Not.Null.And.Not.Empty);
                Assert.That(updatedJob.CompletedAt, Is.Not.Null);
            }
            _notifierMock.Verify(n => n.NotifyAsync(It.Is<Job>(j => j.Id == jobId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task StartAsync_PendingJob_EnqueuesAndProcessesIt()
        {
            var jobId = Guid.NewGuid();
            var pendingJob = new Job { Id = jobId, Status = JobStatus.Pending };

            _jobRepoMock.Setup(r => r.GetOrphanedRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Job>());
            _jobRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Job> { pendingJob });

            var processedTcs = new TaskCompletionSource();
            _processorMock.Setup(p => p.ProcessAsync(jobId, It.IsAny<CancellationToken>()))
                .Callback(() => processedTcs.TrySetResult())
                .Returns(Task.CompletedTask);

            await _dispatcher.StartAsync(CancellationToken.None);
            await Task.WhenAny(processedTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

            _processorMock.Verify(p => p.ProcessAsync(jobId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ProcessJobSafelyAsync_ProcessorThrows_LogsErrorAndStillProcessesNextJob()
        {
            var failingJobId = Guid.NewGuid();
            var succeedingJobId = Guid.NewGuid();

            _jobRepoMock.Setup(r => r.GetOrphanedRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Job>());
            _jobRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Job>
                {
                    new() { Id = failingJobId, Status = JobStatus.Pending },
                    new() { Id = succeedingJobId, Status = JobStatus.Pending }
                });

            _processorMock.Setup(p => p.ProcessAsync(failingJobId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var succeededTcs = new TaskCompletionSource();
            _processorMock.Setup(p => p.ProcessAsync(succeedingJobId, It.IsAny<CancellationToken>()))
                .Callback(() => succeededTcs.TrySetResult())
                .Returns(Task.CompletedTask);

            await _dispatcher.StartAsync(CancellationToken.None);
            await Task.WhenAny(succeededTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));

            _processorMock.Verify(p => p.ProcessAsync(failingJobId, It.IsAny<CancellationToken>()), Times.Once);
            _processorMock.Verify(p => p.ProcessAsync(succeedingJobId, It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
