using Common.Constants.Units;

namespace RecipeBook.Data.Entities
{
    public sealed class Unit : Common.Data.Entities.Entity<Guid>
    {
        public string? Name { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }

        public override string ToString() => $"{Name} ({UnitType})";
    }
}
