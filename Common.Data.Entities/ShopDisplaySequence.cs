using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class ShopDisplaySequence
    {
        public int Value { get; set; }

        [ForeignKey("ShopId")]
        public Shop? Shop { get; private set; }
        public int ShopId { get; set; }

        [ForeignKey("ProductCategoryId")]
        public ProductCategory? ProductCategory { get; private set; }
        public int ProductCategoryId { get; set; }
    }
}
