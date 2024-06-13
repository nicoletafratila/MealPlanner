using Common.Constants.Units;

namespace Common.Data.Profiles.Converters
{
    public class MassConverter : IMassConverter
    {
        public Dictionary<MassUnit, decimal> Conversions = new Dictionary<MassUnit, decimal>()
        {
            { MassUnit.kg, 1 },
            { MassUnit.gr, 1000 }
        };

        public decimal Convert(decimal fromValue, MassUnit fromUnit, MassUnit toUnit)
        {
            decimal workingValue;

            if (fromUnit == MassUnit.kg)
                workingValue = fromValue;
            else
                workingValue = fromValue / Conversions[fromUnit];

            if (toUnit == MassUnit.kg)
                workingValue = workingValue * Conversions[toUnit];

            return workingValue;
        }
    }
}
