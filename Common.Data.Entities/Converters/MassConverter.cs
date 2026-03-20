using Common.Constants.Units;

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
                throw new NotSupportedException($"Conversion from mass unit '{fromUnit}' is not supported.");

            if (!FactorsToKg.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException($"Conversion to mass unit '{toUnit}' is not supported.");

            // to kg, then to target
            var valueInKg = fromValue * fromFactor;
            return valueInKg / toFactor;
        }
    }
}