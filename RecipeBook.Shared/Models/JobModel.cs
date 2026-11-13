using Common.Models;
using RecipeBook.Shared.Enums;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Admin list-row DTO for a share-all background job, used by the Share Job history view.
    /// </summary>
    public class JobModel : BaseModel
    {
        public Guid Id { get; set; }

        public string? OwnerUserId { get; set; }

        public string? TargetUserId { get; set; }

        public JobStatus Status { get; set; }

        public int TotalCount { get; set; }

        public int ProcessedCount { get; set; }

        public int PercentComplete { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
