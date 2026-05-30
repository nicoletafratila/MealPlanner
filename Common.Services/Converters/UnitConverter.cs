using Common.Constants;
using Common.Constants.Units;
using Common.Services.Converters.Resources;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Services.Converters
{
    public static class UnitConverter
    {
        public static decimal Convert(decimal fromValue, Unit fromUnit, Unit toUnit)
        {
            ArgumentNullException.ThrowIfNull(fromUnit);
            ArgumentNullException.ThrowIfNull(toUnit);
            return ConvertCore(fromValue, fromUnit.Name!, fromUnit.UnitType, toUnit.Name!, toUnit.UnitType);
        }

        public static decimal Convert(decimal fromValue, UnitModel fromUnit, UnitModel toUnit)
        {
            ArgumentNullException.ThrowIfNull(fromUnit);
            ArgumentNullException.ThrowIfNull(toUnit);
            return ConvertCore(fromValue, fromUnit.Name, fromUnit.UnitType, toUnit.Name, toUnit.UnitType);
        }

        private static decimal ConvertCore(decimal fromValue, string fromName, UnitType fromType, string toName, UnitType toType)
        {
            if (fromType != toType)
                throw new InvalidOperationException(string.Format(ConverterMessages.UnitTypeMismatch, fromType, toType));

            return fromType switch
            {
                UnitType.Weight =>
                    MassConverter.Convert(
                        fromValue,
                        fromName.ToEnum<MassUnit>(),
                        toName.ToEnum<MassUnit>()),

                UnitType.Liquid =>
                    LiquidConverter.Convert(
                        fromValue,
                        fromName.ToEnum<LiquidUnit>(),
                        toName.ToEnum<LiquidUnit>()),

                UnitType.Volume =>
                    VolumeConverter.Convert(
                        fromValue,
                        fromName.ToEnum<VolumeUnit>(),
                        toName.ToEnum<VolumeUnit>()),

                UnitType.Piece => fromValue,

                _ => throw new NotSupportedException(string.Format(ConverterMessages.UnitTypeNotSupported, fromType))
            };
        }
    }
}
