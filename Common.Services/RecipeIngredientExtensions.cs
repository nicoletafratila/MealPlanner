using Common.Data.Entities;
using Common.Services.Converters;
using Common.Services.Converters.Resources;

namespace Common.Services
{
    public static class RecipeIngredientExtensions
    {
        public static ShoppingListProduct ToShoppingListProduct(this RecipeIngredient ingredient, int displaySequence)
        {
            if (ingredient.Product is null)
                throw new InvalidOperationException(ConverterMessages.ProductMustBeSet);

            if (ingredient.Product.BaseUnit is null)
                throw new InvalidOperationException(ConverterMessages.ProductBaseUnitMustBeSet);

            if (ingredient.Unit is null)
                throw new InvalidOperationException(ConverterMessages.UnitMustBeSet);

            var convertedQuantity = UnitConverter.Convert(ingredient.Quantity, ingredient.Unit, ingredient.Product.BaseUnit);

            return new ShoppingListProduct
            {
                ProductId = ingredient.ProductId,
                Product = ingredient.Product,
                Quantity = convertedQuantity,
                UnitId = ingredient.Product.BaseUnit.Id,
                Unit = ingredient.Product.BaseUnit,
                Collected = false,
                DisplaySequence = displaySequence
            };
        }
    }
}
