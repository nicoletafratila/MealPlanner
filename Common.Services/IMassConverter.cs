using Common.Constants;

namespace Common.Services
{
    public interface IMassConverter
    {
        public double Convert(double fromValue, MassUnit fromUnit, MassUnit toUnit);
    }
}
