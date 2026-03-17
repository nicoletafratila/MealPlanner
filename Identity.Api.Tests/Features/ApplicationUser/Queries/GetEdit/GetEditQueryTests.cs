using Identity.Api.Features.ApplicationUser.Queries.GetEdit;

namespace Identity.Api.Tests.Features.ApplicationUser.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesNameToNull()
        {
            var query = new GetEditQuery();
            Assert.That(query.Name, Is.Null);
        }

        [Test]
        public void Ctor_SetsName()
        {
            var query = new GetEditQuery("alice");
            Assert.That(query.Name, Is.EqualTo("alice"));
        }

        [Test]
        public void Can_Set_And_Get_Name()
        {
            var query = new GetEditQuery
            {
                Name = "bob"
            };
            Assert.That(query.Name, Is.EqualTo("bob"));
        }
    }
}