using Common.Constants.Units;

namespace Common.Data.Entities.Tests
{
    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void DefaultCtor_Sets_Expected_Defaults()
        {
            // Act
            var unit = new Unit();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(unit.Id, Is.Zero);
                Assert.That(unit.Name, Is.EqualTo(string.Empty));
                Assert.That(unit.UnitType, Is.Default);
            }
        }

        [Test]
        public void Properties_Can_Be_Set_And_Read()
        {
            // Arrange
            var expectedType = (UnitType)1;

            // Act
            var unit = new Unit
            {
                Id = 5,
                Name = "Test unit",
                UnitType = expectedType
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(unit.Id, Is.EqualTo(5));
                Assert.That(unit.Name, Is.EqualTo("Test unit"));
                Assert.That(unit.UnitType, Is.EqualTo(expectedType));
            }
        }

        [Test]
        public void ToString_Contains_Name_And_UnitType()
        {
            // Arrange
            var type = (UnitType)2;
            var unit = new Unit
            {
                Name = "MyUnit",
                UnitType = type
            };

            // Act
            var text = unit.ToString();

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(text, Does.Contain("MyUnit"));
                Assert.That(text, Does.Contain(type.ToString()));
            }
        }
    }
}