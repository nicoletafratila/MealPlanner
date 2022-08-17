using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class ShoppingListModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<IngredientModel>? Ingredients { get; set; }
    }
}
