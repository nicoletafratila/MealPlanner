using RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.UpdateAll
{
    [TestFixture]
    public class UpdateAllCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelsToNull()
        {
            // Act
            var command = new UpdateAllCommand();

            // Assert
            Assert.That(command.Models, Is.Null);
        }

        [Test]
        public void Ctor_SetsModels()
        {
            // Arrange
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 1, Name = "Cat1", DisplaySequence = 1 },
                new() { Id = 2, Name = "Cat2", DisplaySequence = 2 }
            };

            // Act
            var command = new UpdateAllCommand(models);

            // Assert
            Assert.That(command.Models, Is.SameAs(models));
        }

        [Test]
        public void Ctor_NullModels_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UpdateAllCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Models_Property()
        {
            // Arrange
            var command = new UpdateAllCommand();
            var models = new List<RecipeCategoryModel>
            {
                new() { Id = 3, Name = "Cat3", DisplaySequence = 3 }
            };

            // Act
            command.Models = models;

            // Assert
            Assert.That(command.Models, Is.SameAs(models));
        }
    }
}