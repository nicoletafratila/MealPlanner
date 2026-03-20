using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public sealed class ShopDisplaySequence
    {
        public int Value { get; set; }

        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }
        public int ShopId { get; set; }

        [ForeignKey(nameof(ProductCategoryId))]
        public ProductCategory? ProductCategory { get; set; }
        public int ProductCategoryId { get; set; }

        public override string ToString() =>
            $"ShopId={ShopId}, CategoryId={ProductCategoryId}, Value={Value}";
    }
}