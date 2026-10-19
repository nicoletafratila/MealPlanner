using Common.Models;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Response returned when a share-all job has been accepted and queued for background processing.
    /// </summary>
    public class JobStartedModel : CommandResponse
    {
        public Guid? JobId { get; set; }
    }
}
