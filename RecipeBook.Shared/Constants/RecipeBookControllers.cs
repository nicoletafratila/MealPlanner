namespace RecipeBook.Shared.Constants
{
    public class RecipeBookControllers
    {
        public const string Recipe = "Recipe";
        public const string RecipeCategory = "RecipeCategory";
        public const string Product = "Product";
        public const string ProductCategory = "ProductCategory";
        public const string Unit = "Unit";

        public const string RecipeUrl = "api/recipe";
        public const string RecipeCategoryUrl = "api/recipecategory";
        public const string ProductUrl = "api/product";
        public const string ProductCategoryUrl = "api/productcategory";
        public const string UnitUrl = "api/unit";

        // Sub-routes shared by all RecipeBook controllers
        public const string EditRoute = "edit";
        public const string SearchRoute = "search";
        public const string UpdateAllRoute = "updateAll";
        public const string ShoppingListProductsRoute = "shoppingListProducts";
    }
}
