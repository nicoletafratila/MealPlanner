namespace Common.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string? Name { get; set; }
        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; }

        public ShoppingList MakeShoppingList()
        {
            var list = new ShoppingList();
            list.Name = "Shopping list details for " + Name;
            var products = new List<ShoppingListProduct>();
            foreach (var item in MealPlanRecipes!)
            {
                foreach (var i in item.Recipe!.RecipeIngredients!)
                {
                    var existingProduct = products.FirstOrDefault(x => x.ProductId == i.ProductId);
                    if (existingProduct == null)
                    {
                        products.Add(i.ToShoppingListProduct());
                    }
                    else
                        existingProduct.Quantity += i.Quantity;
                }
            }
            list.Products = products;
            return list;
        }
    }
}
