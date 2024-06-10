using Common.Constants.Units;

namespace Common.Services
{
    public interface IUnitConverter
    {
        public decimal Convert(decimal fromValue, string fromUnit, string toUnit, UnitType unitType);
    }
}
