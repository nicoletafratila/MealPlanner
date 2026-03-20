using Common.Constants;
using Common.Constants.Units;

namespace Common.Data.Entities.Converters
{
    public static class UnitConverter
    {
        public static decimal Convert(decimal fromValue, Unit fromUnit, Unit toUnit)
        {
            ArgumentNullException.ThrowIfNull(fromUnit);
            ArgumentNullException.ThrowIfNull(toUnit);

            if (fromUnit.UnitType != toUnit.UnitType)
                throw new InvalidOperationException( $"Cannot convert from unit type {fromUnit.UnitType} to {toUnit.UnitType}.");

            return fromUnit.UnitType switch
            {
                UnitType.Weight =>
                    MassConverter.Convert(
                        fromValue,
                        fromUnit.Name!.ToEnum<MassUnit>(),
                        toUnit.Name!.ToEnum<MassUnit>()),

                UnitType.Liquid =>
                    LiquidConverter.Convert(
                        fromValue,
                        fromUnit.Name!.ToEnum<LiquidUnit>(),
                        toUnit.Name!.ToEnum<LiquidUnit>()),

                UnitType.Volume =>
                    VolumeConverter.Convert(
                        fromValue,
                        fromUnit.Name!.ToEnum<VolumeUnit>(),
                        toUnit.Name!.ToEnum<VolumeUnit>()),

                UnitType.Piece => fromValue, // identity

                _ => throw new NotSupportedException($"Unit type '{fromUnit.UnitType}' is not supported.")
            };
        }
    }
}