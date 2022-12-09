namespace RecipeBook.Shared.Models
{
    public class IngredientModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public int DisplaySequence { get; set; }
        //public IngredientCategoryModel Category { get; set; }
    }
}
