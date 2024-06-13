using Common.Constants;
using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public class UnitConverter 
    {
        public static decimal Convert(decimal fromValue, Unit fromUnit, Unit toUnit)
        {
            switch (fromUnit.UnitType)
            {
                case UnitType.Mass:
                    if (toUnit.UnitType == UnitType.Mass)
                        return MassConverter.Convert(fromValue, fromUnit.Name!.ToEnum<MassUnit>(), toUnit.Name!.ToEnum<MassUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Liquid:
                    if (toUnit.UnitType == UnitType.Liquid)
                        return LiquidConverter.Convert(fromValue, fromUnit.Name!.ToEnum<LiquidUnit>(), toUnit.Name!.ToEnum<LiquidUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.All:
                    if (toUnit.UnitType == UnitType.All)
                        return fromValue;
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Piece:
                    if (toUnit.UnitType == UnitType.Piece)
                        return fromValue;
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                default:
                    return fromValue;
            }
        }
    }
}
