using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public class VolumeConverter
    {
        public static Dictionary<VolumeUnit, decimal> conversions = new Dictionary<VolumeUnit, decimal>()
        {
            { VolumeUnit.tsp, 1 },
            { VolumeUnit.tbsp, 0.333333m },
            { VolumeUnit.cup, 0.020833m }
        };

        public static decimal Convert(decimal fromValue, VolumeUnit fromUnit, VolumeUnit toUnit)
        {
            decimal workingValue;

            if (fromUnit == VolumeUnit.tsp)
                workingValue = fromValue;
            else
                workingValue = fromValue / conversions[fromUnit];

            if (toUnit == VolumeUnit.tsp)
                workingValue = workingValue * conversions[toUnit];

            return workingValue;
        }
    }
}
