using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public class MassConverter 
    {
        public static Dictionary<MassUnit, decimal> conversions = new Dictionary<MassUnit, decimal>()
        {
            { MassUnit.kg, 1 },
            { MassUnit.gr, 1000 }
        };

        public static decimal Convert(decimal fromValue, MassUnit fromUnit, MassUnit toUnit)
        {
            decimal workingValue;

            if (fromUnit == MassUnit.kg)
                workingValue = fromValue;
            else
                workingValue = fromValue / conversions[fromUnit];

            if (toUnit == MassUnit.kg)
                workingValue = workingValue * conversions[toUnit];

            return workingValue;
        }
    }
}
