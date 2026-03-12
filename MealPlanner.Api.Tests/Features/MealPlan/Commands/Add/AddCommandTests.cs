using MealPlanner.Api.Features.MealPlan.Commands.Add;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Add
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new AddCommand();
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            var model = new MealPlanEditModel { Id = 0, Name = "Plan1" };

            var command = new AddCommand(model);

            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new AddCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Model()
        {
            var command = new AddCommand();
            var model = new MealPlanEditModel { Id = 1, Name = "PlanX" };

            command.Model = model;

            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}