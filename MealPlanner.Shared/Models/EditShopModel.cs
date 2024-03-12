using System.ComponentModel.DataAnnotations;
using Common.Shared;
using Common.Validators;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class EditShopModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The shop requires at least product category order.")]
        public IList<ShopDisplaySequenceModel>? DisplaySequence { get; set; }

        public EditShopModel()
        { }

        public EditShopModel(IList<ProductCategoryModel>? categories)
        {
            var index = 1;
            DisplaySequence = new List<ShopDisplaySequenceModel>();
            foreach (var item in categories!)
            {
                DisplaySequence.Add(new ShopDisplaySequenceModel
                {
                    ShopId = Id,
                    ProductCategory = item,
                    Value = index++,
                });
            }
        }
    }
}
