namespace RecipeBook.Api.Data.Entities
{
    public class Ingredient : Entity<int>
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public List<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
