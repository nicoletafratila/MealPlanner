namespace RecipeBook.Api.Data.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public List<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
