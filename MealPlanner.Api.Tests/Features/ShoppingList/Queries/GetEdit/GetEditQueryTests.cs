using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToZero()
        {
            // Act
            var query = new GetEditQuery();

            // Assert
            Assert.That(query.Id, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            const int id = 5;

            // Act
            var query = new GetEditQuery(id);

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            // Arrange
            var query = new GetEditQuery
            {
                // Act
                Id = 42
            };

            // Assert
            Assert.That(query.Id, Is.EqualTo(42));
        }
    }
}