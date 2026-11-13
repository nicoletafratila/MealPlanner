using RecipeBook.Data.Entities.Enums;

namespace RecipeBook.Data.Entities
{
    public class Job : Common.Data.Entities.Entity<Guid>
    {
        public string? OwnerUserId { get; set; }

        public string? TargetUserId { get; set; }

        public string? FiltersJson { get; set; }

        public JobStatus Status { get; set; }

        public int TotalCount { get; set; }

        public int ProcessedCount { get; set; }

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public override string ToString() =>
            $"Job (Id: {Id}, OwnerUserId: {OwnerUserId}, Status: {Status})";
    }
}
