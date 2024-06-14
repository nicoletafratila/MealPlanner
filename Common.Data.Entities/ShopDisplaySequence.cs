using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShopDisplaySequence
    {
        public int Value { get; set; }

        [ForeignKey("ShopId")]
        public Shop? Shop { get; set; }
        public int ShopId { get; set; }

        [ForeignKey("ProductCategoryId")]
        public ProductCategory? ProductCategory { get; set; }
        public int ProductCategoryId { get; set; }
    }
}
