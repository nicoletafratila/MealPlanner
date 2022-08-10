namespace RecipeBook.Api.Data.Entities
{
    public class Recipe : Entity<int>
    {
        public string Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public List<RecipeIngredient>? RecipeIngredients { get; set; }
    }
}
