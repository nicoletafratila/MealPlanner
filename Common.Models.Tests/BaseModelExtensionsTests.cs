namespace Common.Models.Tests
{
    [TestFixture]
    public class BaseModelExtensionsTests
    {
        private sealed class TestModel : BaseModel
        {
            public string Name { get; set; } = string.Empty;
        }

        [Test]
        public void SetIndexes_Throws_When_Models_Is_Null()
        {
            IList<TestModel>? models = null;

            Assert.That(
                () => BaseModelExtensions.SetIndexes(models!),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetIndexes_Assigns_Sequential_Indexes_Starting_From_1()
        {
            // Arrange
            var models = new List<TestModel>
            {
                new() { Name = "A" },
                new() { Name = "B" },
                new() { Name = "C" }
            };

            // Act
            models.SetIndexes();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(models[0].Index, Is.EqualTo(1));
                Assert.That(models[1].Index, Is.EqualTo(2));
                Assert.That(models[2].Index, Is.EqualTo(3));
            }
        }

        [Test]
        public void SetIndexes_Overwrites_Existing_Indexes()
        {
            var models = new List<TestModel>
            {
                new() { Name = "A", Index = 10 },
                new() { Name = "B", Index = 20 }
            };

            models.SetIndexes();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(models[0].Index, Is.EqualTo(1));
                Assert.That(models[1].Index, Is.EqualTo(2));
            }
        }

        [Test]
        public void SetIndexes_Does_Nothing_For_Empty_List()
        {
            var models = new List<TestModel>();

            Assert.That(() => models.SetIndexes(), Throws.Nothing);
            Assert.That(models, Is.Empty);
        }

        [Test]
        public void SetIndexes_Ignores_Null_Entries()
        {
            var models = new List<TestModel>
            {
                new() { Name = "A" },
                null,
                new() { Name = "C" }
            };

            // Act
            models.SetIndexes();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(models[0]!.Index, Is.EqualTo(1));
                Assert.That(models[1], Is.Null);
                Assert.That(models[2]!.Index, Is.EqualTo(3));
            }
        }
    }
}