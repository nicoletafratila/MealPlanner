using MealPlanner.Api.Features.MealPlan.Queries.GetEdit;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetEdit
{
    [TestFixture]
    public class GetEditQueryTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToZero()
        {
            var query = new GetEditQuery();
            Assert.That(query.Id, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_SetsId()
        {
            const int id = 7;
            var query = new GetEditQuery(id);
            Assert.That(query.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            var query = new GetEditQuery
            {
                Id = 42
            };
            Assert.That(query.Id, Is.EqualTo(42));
        }
    }
}