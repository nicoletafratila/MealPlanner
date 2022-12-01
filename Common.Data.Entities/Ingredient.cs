using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Ingredient : Entity<int>
    {
        public string Name { get; set; }
        public string Unit { get; set; }

        [ForeignKey("IngredientCategoryId")]
        public IngredientCategory IngredientCategory { get; private set; }
        public int IngredientCategoryId { get; set; }
    }
}
