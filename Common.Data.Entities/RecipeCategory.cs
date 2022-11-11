namespace Common.Data.Entities
{
    public class RecipeCategory : Entity<int>
    {
        public string Name { get; set; }
        public int DisplaySequence { get; set; }
    }
}
