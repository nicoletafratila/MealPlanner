namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Request body to share a recipe by cloning it into a new recipe owned by another user.
    /// </summary>
    public class RecipeShareModel
    {
        public Guid RecipeId { get; set; }
        public string TargetUserId { get; set; } = string.Empty;
    }
}
