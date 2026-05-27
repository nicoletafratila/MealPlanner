using Common.Constants.Units;
using Common.Services.Converters.Resources;

namespace Common.Services.Converters
{
    public static class MassConverter
    {
        private static readonly Dictionary<MassUnit, decimal> FactorsToKg =
            new()
            {
                { MassUnit.kg, 1m },
                { MassUnit.gr, 0.001m }
            };

        public static decimal Convert(decimal fromValue, MassUnit fromUnit, MassUnit toUnit)
        {
            if (fromUnit == toUnit)
                return fromValue;

            if (!FactorsToKg.TryGetValue(fromUnit, out var fromFactor))
                throw new NotSupportedException(string.Format(ConverterMessages.MassConversionFromNotSupported, fromUnit));

            if (!FactorsToKg.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException(string.Format(ConverterMessages.MassConversionToNotSupported, toUnit));

            var valueInKg = fromValue * fromFactor;
            return valueInKg / toFactor;
        }
    }
}
