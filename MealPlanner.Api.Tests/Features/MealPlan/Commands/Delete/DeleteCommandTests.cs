using MealPlanner.Api.Features.MealPlan.Commands.Delete;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToZero()
        {
            var command = new DeleteCommand();
            Assert.That(command.Id, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_SetsId()
        {
            const int id = 7;
            var command = new DeleteCommand(id);
            Assert.That(command.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            var command = new DeleteCommand
            {
                Id = 42
            };
            Assert.That(command.Id, Is.EqualTo(42));
        }
    }
}