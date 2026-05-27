using Common.Constants.Units;
using Common.Services.Converters.Resources;

namespace Common.Services.Converters
{
    public static class VolumeConverter
    {
        private static readonly Dictionary<VolumeUnit, decimal> FactorsToTsp =
            new()
            {
                { VolumeUnit.tsp, 1m },
                { VolumeUnit.tbsp, 3m },
                { VolumeUnit.cup, 48m }
            };

        public static decimal Convert(decimal fromValue, VolumeUnit fromUnit, VolumeUnit toUnit)
        {
            if (fromUnit == toUnit)
                return fromValue;

            if (!FactorsToTsp.TryGetValue(fromUnit, out var fromFactor))
                throw new NotSupportedException(string.Format(ConverterMessages.VolumeConversionFromNotSupported, fromUnit));

            if (!FactorsToTsp.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException(string.Format(ConverterMessages.VolumeConversionToNotSupported, toUnit));

            var valueInTsp = fromValue * fromFactor;
            return valueInTsp / toFactor;
        }
    }
}
