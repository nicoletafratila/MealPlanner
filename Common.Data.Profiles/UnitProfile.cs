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
                .IgnoreBaseModelMembers()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));

            CreateMap<UnitModel, Unit>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));

            CreateMap<Unit, UnitEditModel>()
                .IgnoreBaseModelMembers()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));

            CreateMap<UnitEditModel, Unit>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, value) => value != null));
        }
    }
}