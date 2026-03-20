using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public static class VolumeConverter
    {
        private static readonly Dictionary<VolumeUnit, decimal> FactorsToTsp =
            new()
            {
                { VolumeUnit.tsp, 1m },
                { VolumeUnit.tbsp, 3m },   // 1 tbsp = 3 tsp
                { VolumeUnit.cup, 48m }    // 1 cup = 48 tsp (16 tbsp * 3 tsp)
            };

        public static decimal Convert(decimal fromValue, VolumeUnit fromUnit, VolumeUnit toUnit)
        {
            if (fromUnit == toUnit)
                return fromValue;

            if (!FactorsToTsp.TryGetValue(fromUnit, out var fromFactor))
                throw new NotSupportedException($"Conversion from volume unit '{fromUnit}' is not supported.");

            if (!FactorsToTsp.TryGetValue(toUnit, out var toFactor))
                throw new NotSupportedException($"Conversion to volume unit '{toUnit}' is not supported.");

            var valueInTsp = fromValue * fromFactor;
            return valueInTsp / toFactor;
        }
    }
}