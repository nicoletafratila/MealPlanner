namespace RecipeBook.Data.Entities
{
    public class RecipeCategory : Common.Data.Entities.Entity<Guid>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public int DisplaySequence { get; set; }

        public override string ToString() => $"{Name} (Id: {Id}, Seq: {DisplaySequence})";
    }
}
