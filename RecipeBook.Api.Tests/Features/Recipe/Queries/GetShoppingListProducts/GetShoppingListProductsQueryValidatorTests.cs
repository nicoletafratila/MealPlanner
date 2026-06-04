using FluentValidation.TestHelper;
using RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts;

namespace RecipeBook.Api.Tests.Features.Recipe.Queries.GetShoppingListProducts
{
    [TestFixture]
    public class GetShoppingListProductsQueryValidatorTests
    {
        private GetShoppingListProductsQueryValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new GetShoppingListProductsQueryValidator();
        }

        [Test]
        public void RecipeId_Empty_HasValidationError()
        {
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = Guid.Empty,
                ShopId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.RecipeId);
        }

        [Test]
        public void ShopId_Empty_HasValidationError()
        {
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = Guid.NewGuid(),
                ShopId = Guid.Empty
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void BothIds_Valid_HasNoValidationErrors()
        {
            var query = new GetShoppingListProductsQuery
            {
                RecipeId = Guid.NewGuid(),
                ShopId = Guid.NewGuid()
            };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.RecipeId);
            result.ShouldNotHaveValidationErrorFor(x => x.ShopId);
        }
    }
}