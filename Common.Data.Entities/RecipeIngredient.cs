using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class RecipeIngredient 
    {
        public decimal Quantity { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; private set; }
        public int RecipeId { get; set; }

        [ForeignKey("IngredientId")]
        public Ingredient? Ingredient { get; private set; }
        public int IngredientId { get; set; }
    }
}
