using Common.Constants.Units;
using Common.Data.Entities.Converters.Resources;

namespace Common.Data.Entities.Converters
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

            // to kg, then to target
            var valueInKg = fromValue * fromFactor;
            return valueInKg / toFactor;
        }
    }
}