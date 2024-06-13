using Common.Constants.Units;

namespace Common.Data.Profiles.Converters
{
    public interface IMassConverter
    {
        public decimal Convert(decimal fromValue, MassUnit fromUnit, MassUnit toUnit);
    }
}
