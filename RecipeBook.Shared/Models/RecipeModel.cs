namespace RecipeBook.Shared.Models
{
    public class RecipeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int DisplaySequence { get; set; }
    }
}
