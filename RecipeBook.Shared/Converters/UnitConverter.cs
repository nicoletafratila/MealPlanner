using AutoMapper;
using Common.Data.DataContext;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Converters
{
    public static class UnitConverter
    {
        private static Func<IMapper> _mapperFactory = () => ServiceLocator.GetInstance<IMapper>();
        private static Func<decimal, Unit, Unit, decimal> _convertCore = (value, from, to) => Common.Data.Entities.Converters.UnitConverter.Convert(value, from, to);

        public static void ConfigureMapperFactory(Func<IMapper> mapperFactory)
        {
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
        }

        public static void ConfigureConverter(Func<decimal, Unit, Unit, decimal> convertCore)
        {
            _convertCore = convertCore ?? throw new ArgumentNullException(nameof(convertCore));
        }

        public static decimal Convert(decimal value, UnitModel fromUnit, UnitModel toUnit)
        {
            ArgumentNullException.ThrowIfNull(fromUnit);
            ArgumentNullException.ThrowIfNull(toUnit);

            var mapper = _mapperFactory() ?? throw new InvalidOperationException("IMapper instance cannot be null.");
            var fromEntity = mapper.Map<Unit>(fromUnit);
            var toEntity = mapper.Map<Unit>(toUnit);

            return _convertCore(value, fromEntity, toEntity);
        }
    }
}