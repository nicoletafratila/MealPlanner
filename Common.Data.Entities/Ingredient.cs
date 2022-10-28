namespace Common.Data.Entities
{
    public class Ingredient : Entity<int>
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public IEnumerable<RecipeIngredient> RecipeIngredients { get; set; }
        public IngredientCategory Category { get; set; }
    }
}
