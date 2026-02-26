using Common.Constants.Units;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Tests.Models
{
    [TestFixture]
    public class UnitModelTests
    {
        [Test]
        public void DefaultCtor_InitializesWithDefaults()
        {
            // Act
            var unit = new UnitModel();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unit.Id, Is.EqualTo(0));
                Assert.That(unit.Name, Is.EqualTo(string.Empty));
                Assert.That(unit.UnitType, Is.EqualTo(default(UnitType)));

                // From BaseModel
                Assert.That(unit.Index, Is.EqualTo(0));
                Assert.That(unit.IsSelected, Is.False);
            });
        }

        [Test]
        public void Ctor_SetsProperties()
        {
            // Arrange
            const int id = 5;
            const string name = "kg";
            const UnitType type = UnitType.Weight;

            // Act
            var unit = new UnitModel(id, name, type);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(unit.Id, Is.EqualTo(id));
                Assert.That(unit.Name, Is.EqualTo(name));
                Assert.That(unit.UnitType, Is.EqualTo(type));
            });
        }

        [Test]
        public void ToString_ReturnsName()
        {
            // Arrange
            var unit = new UnitModel
            {
                Id = 1,
                Name = "Liter",
                UnitType = UnitType.Volume
            };

            // Act
            var text = unit.ToString();

            // Assert
            Assert.That(text, Is.EqualTo("Liter"));
        }

        [Test]
        public void Ctor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UnitModel(1, null!, UnitType.Weight);
            });
        }
    }
}