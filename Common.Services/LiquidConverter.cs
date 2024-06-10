using Common.Constants.Units;

namespace Common.Services
{
    public class LiquidConverter : ILiquidConverter
    {
        public Dictionary<LiquidUnit, double> Conversions = new Dictionary<LiquidUnit, double>()
        {
            { LiquidUnit.l, 1 },
            { LiquidUnit.ml, 1000 }
        };

        public double Convert(double fromValue, LiquidUnit fromUnit, LiquidUnit toUnit)
        {
            double workingValue;

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
