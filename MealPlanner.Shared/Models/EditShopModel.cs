using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class EditShopModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        public IList<ShopDisplaySequenceModel>? DisplaySequence { get; set; }
    }
}
