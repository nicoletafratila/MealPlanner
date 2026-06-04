using RecipeBook.Api.Features.Product.Queries.GetEdit;

namespace RecipeBook.Api.Tests.Features.Product.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToEmpty()
        {
            var query = new GetEditQuery();
            Assert.That(query.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsId()
        {
            var id = Guid.NewGuid();
            var query = new GetEditQuery(id);
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id_Property()
        {
            var id = Guid.NewGuid();
            var query = new GetEditQuery { Id = id };
            Assert.That(query.Id, Is.EqualTo(id));
        }
    }
}
