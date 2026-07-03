using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetEdit
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
        public void Can_Set_And_Get_Id()
        {
            var id = Guid.NewGuid();
            var query = new GetEditQuery
            {
                Id = id
            };
            Assert.That(query.Id, Is.EqualTo(id));
        }
    }
}
