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
                    //Mass to Liquid
                    //Mass to Imperial
                    //Mass to Piece
                    if (toUnit.UnitType == UnitType.Mass)
                        return MassConverter.Convert(fromValue, fromUnit.Name!.ToEnum<MassUnit>(), toUnit.Name!.ToEnum<MassUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Liquid:
                    //Liquid to Mass
                    //Liquid to Imperial
                    //Liquid to Piece
                    if (toUnit.UnitType == UnitType.Liquid)
                        return LiquidConverter.Convert(fromValue, fromUnit.Name!.ToEnum<LiquidUnit>(), toUnit.Name!.ToEnum<LiquidUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Volume:
                    //Volume to Mass
                    //Volume to Liquid
                    //Volume to Piece
                    if (toUnit.UnitType == UnitType.Volume)
                        return VolumeConverter.Convert(fromValue, fromUnit.Name!.ToEnum<VolumeUnit>(), toUnit.Name!.ToEnum<VolumeUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Piece:
                    //Piece to Mass
                    //Piece to Liquid
                    //Piece to Imperial
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
