using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using Moq;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Tests.Resolvers
{
    [TestFixture]
    public class RecipeToEditRecipeModelResolverTests
    {
        [Test]
        public void Resolve_Maps_And_Orders_Correctly()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>(MockBehavior.Strict);

            var ingredient1 = new RecipeIngredient
            {
                Product = new Product { Name = "Banana", ProductCategory = new ProductCategory { Name = "Fruit" } }
            };
            var ingredient2 = new RecipeIngredient
            {
                Product = new Product { Name = "Apple", ProductCategory = new ProductCategory { Name = "Fruit" } }
            };
            var ingredient3 = new RecipeIngredient
            {
                Product = new Product { Name = "Carrot", ProductCategory = new ProductCategory { Name = "Vegetable" } }
            };

            var sourceIngredients = new List<RecipeIngredient> { ingredient1, ingredient2, ingredient3 };

            var mapped1 = new RecipeIngredientEditModel { Product = new ProductModel { Name = "Banana", ProductCategory = new ProductCategoryModel { Name = "Fruit" } } };
            var mapped2 = new RecipeIngredientEditModel { Product = new ProductModel { Name = "Apple", ProductCategory = new ProductCategoryModel { Name = "Fruit" } } };
            var mapped3 = new RecipeIngredientEditModel { Product = new ProductModel { Name = "Carrot", ProductCategory = new ProductCategoryModel { Name = "Vegetable" } } };

            mapperMock.Setup(m => m.Map<RecipeIngredientEditModel>(ingredient1)).Returns(mapped1);
            mapperMock.Setup(m => m.Map<RecipeIngredientEditModel>(ingredient2)).Returns(mapped2);
            mapperMock.Setup(m => m.Map<RecipeIngredientEditModel>(ingredient3)).Returns(mapped3);

            var resolver = new RecipeToEditRecipeModelResolver(mapperMock.Object);
            var context = default(ResolutionContext);

            // Act
            var result = resolver.Resolve(
                new Recipe(),
                new RecipeEditModel(),
                sourceIngredients,
                null,
                context);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(3));

                // Order: by category name, then product name
                Assert.That(result![0].Product!.Name, Is.EqualTo("Apple"));
                Assert.That(result[1].Product!.Name, Is.EqualTo("Banana"));
                Assert.That(result[2].Product!.Name, Is.EqualTo("Carrot"));

                // Ensure indexes assigned: 1, 2, 3
                Assert.That(result[0].Index, Is.EqualTo(1));
                Assert.That(result[1].Index, Is.EqualTo(2));
                Assert.That(result[2].Index, Is.EqualTo(3));
            }

            mapperMock.VerifyAll();
        }

        [Test]
        public void Resolve_Returns_Empty_When_SourceValue_Null_Or_Empty()
        {
            // Arrange
            var resolver = new RecipeToEditRecipeModelResolver(Mock.Of<IMapper>());
            var context = default(ResolutionContext);

            // Act
            var empty = resolver.Resolve(new Recipe(), new RecipeEditModel(), null, null, context);
            var emptyList = resolver.Resolve(new Recipe(), new RecipeEditModel(), [], null, context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(empty, Is.Empty);
                Assert.That(emptyList, Is.Empty);
            }
        }
    }
}