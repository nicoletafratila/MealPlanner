using Common.Constants.Units;

namespace Common.Services
{
    public interface ILiquidConverter
    {
        public decimal Convert(decimal fromValue, LiquidUnit fromUnit, LiquidUnit toUnit);
    }
}
