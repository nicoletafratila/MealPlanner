using Common.Constants.Units;

namespace Common.Services
{
    public class LiquidConverter : ILiquidConverter
    {
        public Dictionary<LiquidUnit, decimal> Conversions = new Dictionary<LiquidUnit, decimal>()
        {
            { LiquidUnit.l, 1 },
            { LiquidUnit.ml, 1000 }
        };

        public decimal Convert(decimal fromValue, LiquidUnit fromUnit, LiquidUnit toUnit)
        {
            decimal workingValue;

            if (fromUnit == LiquidUnit.l)
                workingValue = fromValue;
            else
                workingValue = fromValue / Conversions[fromUnit];

            if (toUnit == LiquidUnit.l)
                workingValue = workingValue * Conversions[toUnit];

            return workingValue;
        }
    }
}
