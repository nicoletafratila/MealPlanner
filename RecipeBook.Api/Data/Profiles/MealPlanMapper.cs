using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class MealPlanMapper : Profile
    {
        //private readonly IMapper _mapper;

        //public MealPlanMapper(IMapper mapper)
        public MealPlanMapper()
        {
            //_mapper = mapper;

            CreateMap<MealPlan, EditMealPlanModel>()
               //.ForMember(model => model.Recipes, opt => opt.MapFrom(data => data.MealPlanRecipes.Select(item => _mapper.Map<EditRecipeModel>(item.Recipe))))
               .ReverseMap()
               .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<MealPlan, MealPlanModel>()
                .ReverseMap()
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
