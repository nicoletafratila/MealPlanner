using System.ComponentModel.DataAnnotations;
using Common.Models;
using Common.Validators;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class ShopEditModel : BaseModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The shop requires at least product category order.")]
        public IList<ShopDisplaySequenceEditModel>? DisplaySequence { get; set; }

        public ShopEditModel()
        { }

        public ShopEditModel(IList<ProductCategoryModel>? categories)
        {
            var index = 1;
            DisplaySequence = new List<ShopDisplaySequenceEditModel>();
            foreach (var item in categories!)
            {
                DisplaySequence.Add(new ShopDisplaySequenceEditModel
                {
                    Index = index,
                    ShopId = Id,
                    ProductCategory = item,
                    Value = index++,
                });
            }
        }

        public ShopDisplaySequenceEditModel? GetDisplaySequence(int? categoryId)
        {
            return DisplaySequence?.FirstOrDefault(i => i.ProductCategory!.Id == categoryId);
        }
    }
}
