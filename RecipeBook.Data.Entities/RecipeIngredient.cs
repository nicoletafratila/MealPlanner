using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeBook.Data.Entities
{
    public class RecipeIngredient
    {
        [ForeignKey(nameof(RecipeId))]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        [ForeignKey(nameof(UnitId))]
        public Unit? Unit { get; set; }
        public Guid UnitId { get; set; }
    }
}
