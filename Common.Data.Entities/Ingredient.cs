using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Ingredient : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; private set; }
        public int UnitId { get; set; }

        [ForeignKey("IngredientCategoryId")]
        public IngredientCategory? IngredientCategory { get; private set; }
        public int IngredientCategoryId { get; set; }
    }
}
