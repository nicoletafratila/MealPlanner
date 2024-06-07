using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Product : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }

        [ForeignKey("BaseUnitId")]
        public Unit? BaseUnit { get; private set; }
        public int BaseUnitId { get; set; }

        [ForeignKey("ProductCategoryId")]
        public ProductCategory? ProductCategory { get; private set; }
        public int ProductCategoryId { get; set; }
    }
}
