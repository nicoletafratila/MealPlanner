using AutoMapper;
using Common.Constants.Units;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<Unit, UnitModel>()
               .ForMember(model => model.UnitType, opt => opt.MapFrom(data => data.UnitType.ToString()))
               .ReverseMap()
               .ForMember(data => data.UnitType, opt => opt.MapFrom(model => (UnitModel)Enum.Parse(typeof(UnitType), model.UnitType!)));

            CreateMap<Unit, UnitEditModel>()
               .ReverseMap();
        }
    }
}
