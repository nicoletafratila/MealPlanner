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
        public void MealPlanId_Zero_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = 0,
                ShopId = 1
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void MealPlanId_Negative_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = -1,
                ShopId = 1
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MealPlanId);
        }

        [Test]
        public void ShopId_Zero_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = 1,
                ShopId = 0
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void ShopId_Negative_HasValidationError()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = 1,
                ShopId = -2
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ShopId);
        }

        [Test]
        public void BothIds_GreaterThanZero_HasNoValidationErrors()
        {
            // Arrange
            var query = new GetShoppingListProductsQuery
            {
                MealPlanId = 3,
                ShopId = 4
            };

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.MealPlanId);
            result.ShouldNotHaveValidationErrorFor(x => x.ShopId);
        }
    }
}