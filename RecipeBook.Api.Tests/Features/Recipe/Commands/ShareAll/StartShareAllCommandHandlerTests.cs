using Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using RecipeBook.Api.Features.Recipe.Commands.ShareAll;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Tests.Features.Recipe.Commands.ShareAll
{
    [TestFixture]
    public class StartShareAllCommandHandlerTests
    {
        private Mock<IJobRepository> _jobRepoMock = null!;
        private Mock<IJobQueue> _jobQueueMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private Mock<ILogger<StartShareAllCommandHandler>> _loggerMock = null!;
        private StartShareAllCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>(MockBehavior.Strict);
            _jobQueueMock = new Mock<IJobQueue>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);
            _loggerMock = new Mock<ILogger<StartShareAllCommandHandler>>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new StartShareAllCommandHandler(
                _jobRepoMock.Object, _jobQueueMock.Object, _currentUserMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullJobRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new StartShareAllCommandHandler(null!, _jobQueueMock.Object, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullJobQueue_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new StartShareAllCommandHandler(_jobRepoMock.Object, null!, _currentUserMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new StartShareAllCommandHandler(_jobRepoMock.Object, _jobQueueMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new StartShareAllCommandHandler(_jobRepoMock.Object, _jobQueueMock.Object, _currentUserMock.Object, null!));
        }

        [Test]
        public async Task Handle_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_NoCurrentUser_ReturnsFailedResponse_AndDoesNotPersistOrEnqueue()
        {
            _currentUserMock.Setup(s => s.UserId).Returns((string?)null);
            var command = new StartShareAllCommand { TargetUserId = "user2" };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("User id is required."));
            }

            _jobRepoMock.Verify(r => r.AddAsync(It.IsAny<Data.Entities.Job>(), It.IsAny<CancellationToken>()), Times.Never);
            _jobQueueMock.Verify(q => q.Enqueue(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task Handle_ValidCommand_PersistsPendingJob_Enqueues_AndReturnsStartedModel()
        {
            var command = new StartShareAllCommand { TargetUserId = "user2" };
            var jobId = Guid.NewGuid();

            Data.Entities.Job? savedJob = null;
            _jobRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Job>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Job, CancellationToken>((job, _) => savedJob = job)
                .ReturnsAsync((Data.Entities.Job job, CancellationToken _) =>
                {
                    job.Id = jobId;
                    return job;
                });

            _jobQueueMock.Setup(q => q.Enqueue(jobId));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.JobId, Is.EqualTo(jobId));
                Assert.That(savedJob, Is.Not.Null);
                Assert.That(savedJob!.OwnerUserId, Is.EqualTo("user1"));
                Assert.That(savedJob.TargetUserId, Is.EqualTo("user2"));
                Assert.That(savedJob.Status, Is.EqualTo(JobStatus.Pending));
                Assert.That(savedJob.FiltersJson, Is.Null);
            }

            _jobQueueMock.Verify(q => q.Enqueue(jobId), Times.Once);
        }

        [Test]
        public async Task Handle_WithFilters_SerializesFiltersToJson()
        {
            var filters = new List<Common.Pagination.FilterItem> { new("Name", "Soup", Common.Pagination.FilterOperator.Contains) };
            var command = new StartShareAllCommand { TargetUserId = "user2", Filters = filters };

            Data.Entities.Job? savedJob = null;
            _jobRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Job>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Job, CancellationToken>((job, _) => savedJob = job)
                .ReturnsAsync((Data.Entities.Job job, CancellationToken _) =>
                {
                    job.Id = Guid.NewGuid();
                    return job;
                });

            _jobQueueMock.Setup(q => q.Enqueue(It.IsAny<Guid>()));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);
            Assert.That(savedJob!.FiltersJson, Does.Contain("Soup"));
        }

        [Test]
        public async Task Handle_WithJsonElementFilterValue_NormalizesBeforeSerializing()
        {
            // ASP.NET model-binds [FromBody] FilterItem.Value via System.Text.Json, so it arrives as a JsonElement
            // rather than a plain CLR value - this reproduces that shape instead of constructing FilterItem directly.
            var categoryId = Guid.NewGuid();
            var json = $"[{{\"PropertyName\":\"RecipeCategoryId\",\"Value\":\"{categoryId}\",\"Operator\":1}}]";
            var filters = System.Text.Json.JsonSerializer.Deserialize<List<Common.Pagination.FilterItem>>(json)!;
            Assert.That(filters[0].Value, Is.InstanceOf<System.Text.Json.JsonElement>());

            var command = new StartShareAllCommand { TargetUserId = "user2", Filters = filters };

            Data.Entities.Job? savedJob = null;
            _jobRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Job>(), It.IsAny<CancellationToken>()))
                .Callback<Data.Entities.Job, CancellationToken>((job, _) => savedJob = job)
                .ReturnsAsync((Data.Entities.Job job, CancellationToken _) =>
                {
                    job.Id = Guid.NewGuid();
                    return job;
                });

            _jobQueueMock.Setup(q => q.Enqueue(It.IsAny<Guid>()));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result!.Succeeded, Is.True);

            var roundTripped = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Common.Pagination.FilterItem>>(savedJob!.FiltersJson!)!;
            Assert.That(roundTripped[0].Value, Is.EqualTo(categoryId.ToString()));
        }

        [Test]
        public async Task Handle_ExceptionDuringProcessing_LogsError_AndReturnsFailedResponse()
        {
            var command = new StartShareAllCommand { TargetUserId = "user2" };

            _jobRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Data.Entities.Job>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred when saving the recipe."));
            }

            _jobQueueMock.Verify(q => q.Enqueue(It.IsAny<Guid>()), Times.Never);
        }
    }
}
