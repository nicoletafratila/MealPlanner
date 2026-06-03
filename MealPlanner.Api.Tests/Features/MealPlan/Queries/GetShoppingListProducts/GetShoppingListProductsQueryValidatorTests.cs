using FluentValidation.TestHelper;
using MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts;

namespace MealPlanner.Api.Tests.Features.MealPlan.Queries.GetShoppingListProducts
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
        public void MealPlanId_Empty_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = Guid.Empty,
                ShopId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void ShopId_Empty_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = Guid.NewGuid(),
                ShopId = Guid.Empty
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void BothIds_Valid_HasNoValidationErrors()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = Guid.NewGuid(),
                ShopId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.MealPlanId);
            result.ShouldNotHaveValidationErrorFor(x => x.ShopId);
        }
    }
}