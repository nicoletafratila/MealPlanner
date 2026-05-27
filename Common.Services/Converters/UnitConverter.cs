using Common.Constants;
using Common.Constants.Units;
using RecipeBook.Data.Entities;
using Common.Services.Converters.Resources;

namespace Common.Services.Converters
{
    public static class UnitConverter
    {
        public static decimal Convert(decimal fromValue, Unit fromUnit, Unit toUnit)
        {
            ArgumentNullException.ThrowIfNull(fromUnit);
            ArgumentNullException.ThrowIfNull(toUnit);

            if (fromUnit.UnitType != toUnit.UnitType)
                throw new InvalidOperationException(string.Format(ConverterMessages.UnitTypeMismatch, fromUnit.UnitType, toUnit.UnitType));

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

                UnitType.Piece => fromValue,

                _ => throw new NotSupportedException(string.Format(ConverterMessages.UnitTypeNotSupported, fromUnit.UnitType))
            };
        }
    }
}
