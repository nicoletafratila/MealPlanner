using MealPlanner.Api.Features.MealPlan.Commands.Delete;

namespace MealPlanner.Api.Tests.Features.MealPlan.Commands.Delete
{
    [TestFixture]
    public class DeleteCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesIdToEmpty()
        {
            var command = new DeleteCommand();
            Assert.That(command.Id, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void Ctor_SetsId()
        {
            var id = Guid.NewGuid();
            var command = new DeleteCommand(id);
            Assert.That(command.Id, Is.EqualTo(id));
        }

        [Test]
        public void Can_Set_And_Get_Id()
        {
            var id = Guid.NewGuid();
            var command = new DeleteCommand
            {
                Id = id
            };
            Assert.That(command.Id, Is.EqualTo(id));
        }
    }
}
