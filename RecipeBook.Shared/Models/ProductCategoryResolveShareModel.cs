namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Request body to resolve (or create, if missing) the target user's equivalents of a set of product categories.
    /// </summary>
    public class ProductCategoryResolveShareModel
    {
        public IList<Guid> CategoryIds { get; set; } = [];

        public string TargetUserId { get; set; } = string.Empty;
    }
}
