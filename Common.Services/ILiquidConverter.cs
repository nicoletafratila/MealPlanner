using Common.Constants.Units;

namespace Common.Services
{
    public interface ILiquidConverter
    {
        public double Convert(double fromValue, LiquidUnit fromUnit, LiquidUnit toUnit);
    }
}
