using Common.Constants.Units;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class UnitModel : BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public UnitType UnitType { get; set; }

        public UnitModel()
        {
        }

        public UnitModel(int id, string name, UnitType unitType)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UnitType = unitType;
        }

        public override string ToString() => Name;
    }
}