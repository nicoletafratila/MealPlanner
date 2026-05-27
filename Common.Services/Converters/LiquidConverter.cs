using Common.Constants.Units;
using Common.Services.Converters.Resources;

namespace Common.Services.Converters
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
                throw new NotSupportedException(string.Format(ConverterMessages.LiquidConversionFromNotSupported, fromUnit));

            if (!FactorsToL.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException(string.Format(ConverterMessages.LiquidConversionToNotSupported, toUnit));

            var valueInL = fromValue * fromFactor;
            return valueInL / toFactor;
        }
    }
}
