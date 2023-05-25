namespace RecipeBook.Shared.Models
{
    public class IngredientModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public UnitModel? Unit { get; set; }
        public IngredientCategoryModel? IngredientCategory { get; set; }
    }
}
