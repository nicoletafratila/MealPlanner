namespace Common.Data.Entities
{
    public static class RecipeIngredientExtension
    {
        public static ShoppingListProduct ToShoppingListProduct(this RecipeIngredient ingredient)
        {
            var result = new ShoppingListProduct();
            result.ProductId = ingredient.ProductId;
            result.Quantity = ingredient.Quantity;
            result.Collected = false;
            return result;
        }
    }
}
