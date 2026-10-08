namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Request body to share all of the current user's recipes by cloning them into new recipes owned by another user.
    /// </summary>
    public class RecipeShareAllModel
    {
        public string TargetUserId { get; set; } = string.Empty;
    }
}
