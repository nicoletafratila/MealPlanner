using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBook.Api.Data.Entities
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; private set; }
        
        [ForeignKey("IngredientId")]
        public Ingredient Ingredient { get; private set; }
    }
}
