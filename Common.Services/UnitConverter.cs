using Common.Constants;

namespace Common.Services
{
    public class UnitConverter : IUnitConverter
    {
        public double Convert(double fromValue, string fromUnit, string toUnit, UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Mass:
                    return new MassConverter().Convert(fromValue, fromUnit.ToEnum<MassUnit>(), toUnit.ToEnum<MassUnit>());
                case UnitType.Liquid:
                    return new LiquidConverter().Convert(fromValue, fromUnit.ToEnum<LiquidUnit>(), toUnit.ToEnum<LiquidUnit>());
                case UnitType.All:
                case UnitType.Unit:
                default:
                    return fromValue;
            }
        }
    }
}
