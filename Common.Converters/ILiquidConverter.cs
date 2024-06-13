using Common.Constants.Units;

namespace Common.Data.Profiles.Converters
{
    public interface ILiquidConverter
    {
        public decimal Convert(decimal fromValue, LiquidUnit fromUnit, LiquidUnit toUnit);
    }
}
