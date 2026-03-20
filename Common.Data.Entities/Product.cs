using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public sealed class Product : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }

        [ForeignKey(nameof(BaseUnitId))]
        public Unit? BaseUnit { get; set; }
        public int BaseUnitId { get; set; }

        [ForeignKey(nameof(ProductCategoryId))]
        public ProductCategory? ProductCategory { get; set; }
        public int ProductCategoryId { get; set; }

        public override string ToString() =>
            $"{Name} (Id: {Id}, CategoryId: {ProductCategoryId}, BaseUnitId: {BaseUnitId})";
    }
}