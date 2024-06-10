using Common.Constants.Units;

namespace Common.Services
{
    public interface IMassConverter
    {
        public decimal Convert(decimal fromValue, MassUnit fromUnit, MassUnit toUnit);
    }
}
