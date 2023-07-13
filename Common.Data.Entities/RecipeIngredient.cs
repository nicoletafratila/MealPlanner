using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class RecipeIngredient 
    {
        public decimal Quantity { get; set; }
        
        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; private set; }
        public int RecipeId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; private set; }
        public int ProductId { get; set; }
    }
}
