using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using Moq;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class EditRecipeModelToRecipeResolverTests
    {
        [Test]
        public void Resolve_Maps_Ingredients_In_Order()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceIngredients = new List<RecipeIngredientEditModel>
            {
                new() { Index = 1, Quantity = 5, UnitId = 3, Product = new ProductModel { Id= 10 } },
                new() { Index = 2, Quantity = 1, UnitId = 4, Product = new ProductModel { Id = 20 } },
                new() { Index = 3, Quantity = 3, UnitId = 5, Product = new ProductModel { Id = 30 } }
            };

            var mapped1 = new RecipeIngredient { Quantity = 5, UnitId = 3, ProductId = 10 };
            var mapped2 = new RecipeIngredient { Quantity = 1, UnitId = 4, ProductId = 20 };
            var mapped3 = new RecipeIngredient { Quantity = 3, UnitId = 5, ProductId = 30 };

            mapperMock.Setup(m => m.Map<RecipeIngredient>(sourceIngredients[0])).Returns(mapped1);
            mapperMock.Setup(m => m.Map<RecipeIngredient>(sourceIngredients[1])).Returns(mapped2);
            mapperMock.Setup(m => m.Map<RecipeIngredient>(sourceIngredients[2])).Returns(mapped3);

            var resolver = new EditRecipeModelToRecipeResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var editRecipe = new RecipeEditModel
            {
                Ingredients = sourceIngredients
            };

            // Act
            var result = resolver.Resolve(
                source: editRecipe,
                destination: new Recipe(),
                sourceValue: sourceIngredients,
                destValue: null,
                context: context
            );

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(3));
                Assert.That(result![0].Quantity, Is.EqualTo(5));
                Assert.That(result[1].Quantity, Is.EqualTo(1));
                Assert.That(result[2].Quantity, Is.EqualTo(3));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null_Or_Empty()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            var resolver = new EditRecipeModelToRecipeResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var modelNull = new RecipeEditModel { Ingredients = null };
            var modelEmpty = new RecipeEditModel { Ingredients = [] };

            var empty = resolver.Resolve(modelNull, new Recipe(), null, null, context);
            var emptyList = resolver.Resolve(modelEmpty, new Recipe(), [], null, context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(empty, Is.Empty);
                Assert.That(emptyList, Is.Empty);
            }

            mapperMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Resolve_Maps_Ingredient_Values_Correctly()
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var sourceList = new List<RecipeIngredientEditModel>
            {
                new() { Index = 10, Quantity = 2, UnitId = 4, Product = new ProductModel { Id = 99 } }
            };

            var mapped = new RecipeIngredient
            {
                Quantity = 2,
                UnitId = 4,
                ProductId = 99
            };

            mapperMock.Setup(m => m.Map<RecipeIngredient>(sourceList[0]))
                      .Returns(mapped);

            var resolver = new EditRecipeModelToRecipeResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            var editRecipe = new RecipeEditModel
            {
                Ingredients = sourceList
            };

            var result = resolver.Resolve(editRecipe, new Recipe(), sourceList, null, context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result[0].Quantity, Is.EqualTo(2));
                Assert.That(result[0].UnitId, Is.EqualTo(4));
                Assert.That(result[0].ProductId, Is.EqualTo(99));
            }

            mapperMock.VerifyAll();
        }
    }
}