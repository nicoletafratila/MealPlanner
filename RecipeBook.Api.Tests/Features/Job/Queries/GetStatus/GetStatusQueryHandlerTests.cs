using Common.Services;
using Moq;
using RecipeBook.Api.Features.Job.Queries.GetStatus;
using RecipeBook.Api.Repositories;
using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Api.Tests.Features.Job.Queries.GetStatus
{
    [TestFixture]
    public class GetStatusQueryHandlerTests
    {
        private Mock<IJobRepository> _jobRepoMock = null!;
        private Mock<ICurrentUserService> _currentUserMock = null!;
        private GetStatusQueryHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobRepository>(MockBehavior.Strict);
            _currentUserMock = new Mock<ICurrentUserService>(MockBehavior.Loose);

            _currentUserMock.Setup(s => s.UserId).Returns("user1");

            _handler = new GetStatusQueryHandler(_jobRepoMock.Object, _currentUserMock.Object);
        }

        [Test]
        public void Ctor_NullJobRepository_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetStatusQueryHandler(null!, _currentUserMock.Object));
        }

        [Test]
        public void Ctor_NullCurrentUserService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new GetStatusQueryHandler(_jobRepoMock.Object, null!));
        }

        [Test]
        public async Task Handle_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_JobNotFound_ReturnsNull()
        {
            var jobId = Guid.NewGuid();
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync((Data.Entities.Job?)null);

            var result = await _handler.Handle(new GetStatusQuery(jobId), CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Handle_JobOwnedByDifferentUser_ReturnsNull()
        {
            var jobId = Guid.NewGuid();
            var job = new Data.Entities.Job { Id = jobId, OwnerUserId = "someoneElse" };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var result = await _handler.Handle(new GetStatusQuery(jobId), CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Handle_OwnedJob_ReturnsStatusModelWithComputedPercent()
        {
            var jobId = Guid.NewGuid();
            var job = new Data.Entities.Job
            {
                Id = jobId,
                OwnerUserId = "user1",
                Status = JobStatus.Running,
                TotalCount = 10,
                ProcessedCount = 3,
                ErrorMessage = null
            };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var result = await _handler.Handle(new GetStatusQuery(jobId), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.JobId, Is.EqualTo(jobId));
                Assert.That(result.Status, Is.EqualTo(Shared.Enums.JobStatus.Running));
                Assert.That(result.ProcessedCount, Is.EqualTo(3));
                Assert.That(result.TotalCount, Is.EqualTo(10));
                Assert.That(result.PercentComplete, Is.EqualTo(30));
            }
        }

        [Test]
        public async Task Handle_TotalCountZero_PercentCompleteIsZero()
        {
            var jobId = Guid.NewGuid();
            var job = new Data.Entities.Job { Id = jobId, OwnerUserId = "user1", TotalCount = 0, ProcessedCount = 0 };
            _jobRepoMock.Setup(r => r.GetByIdAsync(jobId, It.IsAny<CancellationToken>())).ReturnsAsync(job);

            var result = await _handler.Handle(new GetStatusQuery(jobId), CancellationToken.None);

            Assert.That(result!.PercentComplete, Is.Zero);
        }
    }
}
