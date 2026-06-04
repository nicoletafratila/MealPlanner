using RecipeBook.Api.Features.Product.Commands.Delete;

namespace RecipeBook.Api.Tests.Features.Product.Commands.Delete
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
        public void Can_Set_And_Get_Id_Property()
        {
            var id = Guid.NewGuid();
            var command = new DeleteCommand { Id = id };
            Assert.That(command.Id, Is.EqualTo(id));
        }
    }
}
