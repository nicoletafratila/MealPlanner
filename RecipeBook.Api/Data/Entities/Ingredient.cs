using Common.Data.Data.Entities;

namespace RecipeBook.Api.Data.Entities
{
    public class Ingredient : Entity<int>
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public IEnumerable<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
