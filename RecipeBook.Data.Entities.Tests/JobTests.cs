using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Data.Entities.Tests
{
    [TestFixture]
    public class JobTests
    {
        [Test]
        public void DefaultCtor_Sets_Default_Values()
        {
            var job = new Job();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(job.Id, Is.EqualTo(Guid.Empty));
                Assert.That(job.OwnerUserId, Is.Null);
                Assert.That(job.TargetUserId, Is.Null);
                Assert.That(job.FiltersJson, Is.Null);
                Assert.That(job.Status, Is.EqualTo(JobStatus.Pending));
                Assert.That(job.TotalCount, Is.Zero);
                Assert.That(job.ProcessedCount, Is.Zero);
                Assert.That(job.ErrorMessage, Is.Null);
                Assert.That(job.CompletedAt, Is.Null);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            var id = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var completedAt = createdAt.AddMinutes(5);

            var job = new Job
            {
                Id = id,
                OwnerUserId = "user1",
                TargetUserId = "user2",
                FiltersJson = "[]",
                Status = JobStatus.Running,
                TotalCount = 10,
                ProcessedCount = 3,
                ErrorMessage = "boom",
                CreatedAt = createdAt,
                CompletedAt = completedAt
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(job.Id, Is.EqualTo(id));
                Assert.That(job.OwnerUserId, Is.EqualTo("user1"));
                Assert.That(job.TargetUserId, Is.EqualTo("user2"));
                Assert.That(job.FiltersJson, Is.EqualTo("[]"));
                Assert.That(job.Status, Is.EqualTo(JobStatus.Running));
                Assert.That(job.TotalCount, Is.EqualTo(10));
                Assert.That(job.ProcessedCount, Is.EqualTo(3));
                Assert.That(job.ErrorMessage, Is.EqualTo("boom"));
                Assert.That(job.CreatedAt, Is.EqualTo(createdAt));
                Assert.That(job.CompletedAt, Is.EqualTo(completedAt));
            }
        }

        [Test]
        public void ToString_Contains_Id_OwnerUserId_AndStatus()
        {
            var id = Guid.NewGuid();
            var job = new Job { Id = id, OwnerUserId = "user1", Status = JobStatus.Completed };

            var text = job.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain(id.ToString()));
                Assert.That(text, Does.Contain("user1"));
                Assert.That(text, Does.Contain("Completed"));
            }
        }
    }
}
