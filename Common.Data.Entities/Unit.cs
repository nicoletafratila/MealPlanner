using Common.Constants.Units;

namespace Common.Data.Entities
{
    public sealed class Unit : Entity<int>
    {
        public string? Name { get; set; } = string.Empty;
        public UnitType UnitType { get; set; }

        public override string ToString() => $"{Name} ({UnitType})";
    }
}