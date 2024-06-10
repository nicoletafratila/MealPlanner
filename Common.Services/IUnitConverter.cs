using Common.Constants;

namespace Common.Services
{
    public interface IUnitConverter
    {
        public double Convert(double fromValue, string fromUnit, string toUnit, UnitType unitType);
    }
}
