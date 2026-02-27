using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class MealPlanModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new MealPlanModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(0));
                Assert.That(model.Name, Is.EqualTo(string.Empty));

                // BaseModel defaults
                Assert.That(model.Index, Is.EqualTo(0));
                Assert.That(model.IsSelected, Is.False);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 3;
            const string name = "Weekly Plan";

            // Act
            var model = new MealPlanModel(id, name);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
            });
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new MealPlanModel(1, null!);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var model = new MealPlanModel
            {
                Id = 1,
                Name = "Dinner Plan"
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Dinner Plan"));
        }
    }
}