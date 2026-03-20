namespace Common.Data.Entities
{
    public sealed class RecipeCategory : Entity<int>
    {
        public string? Name { get; set; }
        public int DisplaySequence { get; set; }

        public override string ToString() => $"{Name} (Id: {Id}, Seq: {DisplaySequence})";
    }
}