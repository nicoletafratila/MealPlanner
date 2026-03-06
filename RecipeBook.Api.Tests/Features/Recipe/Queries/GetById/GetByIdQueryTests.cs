using RecipeBook.Api.Features.Recipe.Queries.GetById;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetById
{
    [TestFixture]
    public class GetByIdQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToZero()
        {
            // Act
            var query = new GetByIdQuery();

            // Assert
            Assert.That(query.Id, Is.Zero);
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            const int id = 7;

            // Act
            var query = new GetByIdQuery(id);

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id_Property()
        {
            // Arrange
            var query = new GetByIdQuery
            {
                // Act
                Id = 42
            };

            // Assert
            Assert.That(query.Id, Is.EqualTo(42));
        }
    }
}