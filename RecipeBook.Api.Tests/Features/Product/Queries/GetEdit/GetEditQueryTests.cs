using RecipeBook.Api.Features.Product.Queries.GetEdit;

namespace RecipeBook.Api.Tests.Features.Product.Queries.GetEdit
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
            Assert.That(query.Id, Is.Zero);
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            const int id = 7;

            // Act
            var query = new GetEditQuery(id);

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id_Property()
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