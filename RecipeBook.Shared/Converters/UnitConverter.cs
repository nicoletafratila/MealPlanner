using AutoMapper;
using Common.Data.DataContext;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Shared.Converters
{
    public class UnitConverter 
    {
        public static decimal Convert(decimal fromValue, UnitModel fromUnit, UnitModel toUnit)
        {
            var mapper = ServiceLocator.Current.GetInstance<IMapper>();
            return Common.Data.Entities.Converters.UnitConverter.Convert(fromValue, mapper!.Map<Unit>(fromUnit), mapper!.Map<Unit>(toUnit));
        }
    }
}
