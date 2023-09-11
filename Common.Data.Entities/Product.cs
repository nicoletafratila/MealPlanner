using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class Product : Entity<int>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }

        [ForeignKey("UnitId")]
        public Unit? Unit { get; private set; }
        public int UnitId { get; set; }

        [ForeignKey("ProductCategoryId")]
        public ProductCategory? ProductCategory { get; private set; }
        public int ProductCategoryId { get; set; }
    }
}
