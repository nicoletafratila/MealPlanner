using RecipeBook.Api.Features.Recipe.Queries.GetById;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetById
{
    [TestFixture]
    public class GetByIdQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToEmpty()
        {
            // Act
            var query = new GetByIdQuery();

            // Assert
            Assert.That(query.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsId()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetByIdQuery(id);

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id_Property()
        {
            // Arrange
            var id = Guid.NewGuid();
            var query = new GetByIdQuery
            {
                // Act
                Id = id
            };

            // Assert
            Assert.That(query.Id, Is.EqualTo(id));
        }
    }
}