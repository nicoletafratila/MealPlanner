namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Request body to share a product by cloning it into a new product owned by another user.
    /// </summary>
    public class ProductShareModel
    {
        public Guid ProductId { get; set; }
        public string TargetUserId { get; set; } = string.Empty;
    }
}
