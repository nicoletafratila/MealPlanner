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
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(Guid.Empty));
                Assert.That(model.Name, Is.EqualTo(string.Empty));

                // BaseModel defaults
                Assert.That(model.Index, Is.Zero);
                Assert.That(model.IsSelected, Is.False);
            }
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            const string name = "Weekly Plan";

            // Act
            var model = new MealPlanModel(id, name);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.EqualTo(id));
                Assert.That(model.Name, Is.EqualTo(name));
            }
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new MealPlanModel(Guid.NewGuid(), null!);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var model = new MealPlanModel
            {
                Id = Guid.NewGuid(),
                Name = "Dinner Plan"
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Dinner Plan"));
        }
    }
}