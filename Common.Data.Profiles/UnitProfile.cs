using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class UnitProfile : Profile
    {
        public UnitProfile()
        {
            CreateMap<Unit, UnitModel>()
               .ReverseMap();

            CreateMap<Unit, EditUnitModel>()
               .ReverseMap();
        }
    }
}
