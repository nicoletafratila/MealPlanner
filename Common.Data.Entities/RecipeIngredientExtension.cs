namespace Common.Data.Entities
{
    public static class RecipeIngredientExtension
    {
        public static ShoppingListProduct ToShoppingListProduct(this RecipeIngredient ingredient, int displaySequence)
        {
            var result = new ShoppingListProduct
            {
                ProductId = ingredient.ProductId,
                Quantity = ingredient.Quantity,
                Collected = false,
                DisplaySequence = displaySequence
            };
            return result;
        }
    }
}
