using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<Ingredient, IngredientModel>()
               .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
               .ReverseMap();

            CreateMap<Ingredient, EditIngredientModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
                .ReverseMap()
                .ForMember(data => data.IngredientCategory, opt => opt.Ignore())
                .ForMember(data => data.Unit, opt => opt.Ignore());
        }
    }
}
