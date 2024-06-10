using Common.Constants.Units;

namespace Common.Data.Entities
{
    public class Unit : Entity<int>
    {
        public string? Name { get; set; }
        public UnitType UnitType { get; set; }
    }
}
