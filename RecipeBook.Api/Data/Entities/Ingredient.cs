namespace RecipeBook.Api.Data.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public List<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
