using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public class LiquidConverter 
    {
        private static Dictionary<LiquidUnit, decimal> conversions = new Dictionary<LiquidUnit, decimal>()
        {
            { LiquidUnit.l, 1 },
            { LiquidUnit.ml, 1000 }
        };

        public static decimal Convert(decimal fromValue, LiquidUnit fromUnit, LiquidUnit toUnit)
        {
            decimal workingValue;

            if (fromUnit == LiquidUnit.l)
                workingValue = fromValue;
            else
                workingValue = fromValue / conversions[fromUnit];

            if (toUnit == LiquidUnit.l)
                workingValue = workingValue * conversions[toUnit];

            return workingValue;
        }
    }
}
