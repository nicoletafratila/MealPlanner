namespace Common.Data.Entities
{
    public class IngredientCategory : Entity<int>
    {
        public string Name { get; set; }
        public int DisplaySequence { get; set; }
    }
}
