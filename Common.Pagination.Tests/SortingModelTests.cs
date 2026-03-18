using BlazorBootstrap;

namespace Common.Pagination.Tests
{
    [TestFixture]
    public class SortingModelTests
    {
        [Test]
        public void ObjectInitializer_Sets_All_Properties()
        {
            // Arrange & Act
            var model = new SortingModel
            {
                PropertyName = "Name",
                Direction = SortDirection.Descending
            };

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(model.PropertyName, Is.EqualTo("Name"));
                    Assert.That(model.Direction, Is.EqualTo(SortDirection.Descending));
                });
            }
        }

        [Test]
        public void Direction_Defaults_To_Ascending_When_Not_Specified()
        {
            // Arrange & Act
            var model = new SortingModel
            {
                PropertyName = "CreatedOn"
            };

            // Assert
            Assert.That(model.Direction, Is.EqualTo(SortDirection.Ascending));
        }

        [Test]
        public void ToString_Returns_Readable_Representation()
        {
            // Arrange
            var model = new SortingModel
            {
                PropertyName = "Id",
                Direction = SortDirection.Descending
            };

            // Act
            var result = model.ToString();

            // Assert
            Assert.That(result, Is.EqualTo("Id (Descending)"));
        }
    }
}