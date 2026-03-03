using MealPlanner.Shared.Models;

namespace MealPlanner.Shared.Tests.Models
{
    [TestFixture]
    public class ShoppingListModelTests
    {
        [Test]
        public void DefaultCtor_InitializesDefaults()
        {
            // Act
            var model = new ShoppingListModel();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(model.Id, Is.Zero);
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
            const int id = 5;
            const string name = "Weekly Groceries";

            // Act
            var model = new ShoppingListModel(id, name);

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
                _ = new ShoppingListModel(1, null!);
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var model = new ShoppingListModel
            {
                Id = 1,
                Name = "Party Supplies"
            };

            // Act
            var text = model.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Party Supplies"));
        }
    }
}