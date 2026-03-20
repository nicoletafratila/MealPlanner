using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public static class LiquidConverter
    {
        private static readonly Dictionary<LiquidUnit, decimal> FactorsToL =
            new()
            {
                { LiquidUnit.l, 1m },
                { LiquidUnit.ml, 0.001m }
            };

        public static decimal Convert(decimal fromValue, LiquidUnit fromUnit, LiquidUnit toUnit)
        {
            if (fromUnit == toUnit)
                return fromValue;

            if (!FactorsToL.TryGetValue(fromUnit, out var fromFactor))
                throw new NotSupportedException($"Conversion from liquid unit '{fromUnit}' is not supported.");

            if (!FactorsToL.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException($"Conversion to liquid unit '{toUnit}' is not supported.");

            var valueInL = fromValue * fromFactor;
            return valueInL / toFactor;
        }
    }
}