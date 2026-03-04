using System;
using NUnit.Framework;
using RecipeBook.Api.Features.RecipeCategory.Commands.Add;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Tests.Features.RecipeCategory.Commands.Add
{
    [TestFixture]
    public class AddCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            // Act
            var command = new AddCommand();

            // Assert
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            // Arrange
            var model = new RecipeCategoryEditModel
            {
                Id = 0,
                Name = "Breakfast",
                DisplaySequence = 1
            };

            // Act
            var command = new AddCommand(model);

            // Assert
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
        public void Can_Set_And_Get_Model_Property()
        {
            // Arrange
            var command = new AddCommand();
            var model = new RecipeCategoryEditModel
            {
                Id = 0,
                Name = "Lunch",
                DisplaySequence = 2
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}