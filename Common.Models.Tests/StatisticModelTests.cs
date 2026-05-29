namespace Common.Models.Tests
{
    [TestFixture]
    public class StatisticModelTests
    {
        [Test]
        public void DefaultCtor_InitializesEmptyData()
        {
            // Act
            var model = new StatisticModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Title, Is.Null);
                Assert.That(model.Label, Is.Null);
                Assert.That(model.Data, Is.Not.Null);
                Assert.That(model.Data, Is.Empty);
            }
        }

        [Test]
        public void Properties_AreSettable()
        {
            // Arrange
            var data = new Dictionary<string, double?>
            {
                ["Pasta"] = 10,
                ["Salad"] = 5
            };

            // Act
            var model = new StatisticModel
            {
                Title = "Favorite Recipes",
                Label = "Recipes",
                Data = data
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Title, Is.EqualTo("Favorite Recipes"));
                Assert.That(model.Label, Is.EqualTo("Recipes"));
                Assert.That(model.Data, Is.SameAs(data));
            }
        }
    }
}
