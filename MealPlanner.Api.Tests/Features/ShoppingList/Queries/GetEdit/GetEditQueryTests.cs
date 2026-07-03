using MealPlanner.Api.Features.ShoppingList.Queries.GetEdit;

namespace MealPlanner.Api.Tests.Features.ShoppingList.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToEmpty()
        {
            // Act
            var query = new GetEditQuery();

            // Assert
            Assert.That(query.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetEditQuery(id);

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            // Arrange
            var id = Guid.NewGuid();
            var query = new GetEditQuery
            {
                // Act
                Id = id
            };

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }
    }
}
