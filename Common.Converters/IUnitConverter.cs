using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Converters
{
    public interface IUnitConverter
    {
        public decimal Convert(decimal fromValue, UnitModel fromUnit, UnitModel toUnit);
    }
}
