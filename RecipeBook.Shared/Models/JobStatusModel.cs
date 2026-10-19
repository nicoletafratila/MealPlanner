using RecipeBook.Shared.Enums;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Current progress of a share-all background job. Used both as the GET-status response and the SignalR progress payload.
    /// </summary>
    public class JobStatusModel
    {
        public Guid JobId { get; set; }

        public JobStatus Status { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public int PercentComplete { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
