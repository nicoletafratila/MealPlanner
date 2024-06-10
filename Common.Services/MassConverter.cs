using Common.Constants.Units;

namespace Common.Services
{
    public class MassConverter : IMassConverter
    {
        public Dictionary<MassUnit, double> Conversions = new Dictionary<MassUnit, double>()
        {
            { MassUnit.kg, 1 },
            { MassUnit.gr, 1000 }
        };

        public double Convert(double fromValue, MassUnit fromUnit, MassUnit toUnit)
        {
            double workingValue;

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
