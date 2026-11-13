using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RecipeBook.Data.Entities;
using RecipeBook.Data.Entities.Enums;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles.Tests
{
    [TestFixture]
    public class JobProfileTests
    {
        private IMapper _mapper = null!;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<JobProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Job_To_JobModel_Maps_Properties()
        {
            var entity = new Job
            {
                Id = Guid.NewGuid(),
                OwnerUserId = "owner",
                TargetUserId = "target",
                Status = JobStatus.Running,
                TotalCount = 10,
                ProcessedCount = 4,
                ErrorMessage = "boom",
                CreatedAt = new DateTime(2026, 1, 1),
                CompletedAt = new DateTime(2026, 1, 2)
            };

            var result = _mapper.Map<JobModel>(entity);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Id, Is.EqualTo(entity.Id));
                Assert.That(result.OwnerUserId, Is.EqualTo("owner"));
                Assert.That(result.TargetUserId, Is.EqualTo("target"));
                Assert.That(result.Status, Is.EqualTo(Shared.Enums.JobStatus.Running));
                Assert.That(result.TotalCount, Is.EqualTo(10));
                Assert.That(result.ProcessedCount, Is.EqualTo(4));
                Assert.That(result.PercentComplete, Is.EqualTo(40));
                Assert.That(result.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(result.CreatedAt, Is.EqualTo(entity.CreatedAt));
                Assert.That(result.CompletedAt, Is.EqualTo(entity.CompletedAt));
                Assert.That(result.Index, Is.Zero);
                Assert.That(result.IsSelected, Is.False);
            }
        }

        [Test]
        public void Job_To_JobModel_TotalCountZero_PercentCompleteIsZero()
        {
            var entity = new Job { TotalCount = 0, ProcessedCount = 0 };

            var result = _mapper.Map<JobModel>(entity);

            Assert.That(result.PercentComplete, Is.Zero);
        }
    }
}
