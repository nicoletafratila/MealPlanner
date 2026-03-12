using MealPlanner.Api.Features.MealPlan.Commands.Update;
using MealPlanner.Shared.Models;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Update
{
    [TestFixture]
    public class UpdateCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new UpdateCommand();
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            var model = new MealPlanEditModel { Id = 1, Name = "Plan1" };

            var command = new UpdateCommand(model);

            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UpdateCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Model()
        {
            var command = new UpdateCommand();
            var model = new MealPlanEditModel { Id = 2, Name = "Plan2" };

            command.Model = model;

            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}