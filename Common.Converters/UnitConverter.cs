using Common.Constants;
using Common.Constants.Units;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles.Converters
{
    public class UnitConverter : IUnitConverter
    {
        public decimal Convert(decimal fromValue, UnitModel fromUnit, UnitModel toUnit)
        {
            switch (fromUnit.UnitType)
            {
                case UnitType.Mass:
                    if (toUnit.UnitType == UnitType.Mass)
                        return new MassConverter().Convert(fromValue, fromUnit.Name!.ToEnum<MassUnit>(), toUnit.Name!.ToEnum<MassUnit>());
                    else
                        throw new InvalidOperationException($"Cannot convert from {fromUnit.Name} to {toUnit.Name}");
                case UnitType.Liquid:
                    if (toUnit.UnitType == UnitType.Liquid)
                        return new LiquidConverter().Convert(fromValue, fromUnit.Name!.ToEnum<LiquidUnit>(), toUnit.Name!.ToEnum<LiquidUnit>());
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
