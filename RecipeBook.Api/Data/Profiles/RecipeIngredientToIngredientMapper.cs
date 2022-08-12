using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public static class RecipeIngredientToIngredientMapper
    {
        private static IMapper _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Ingredient, IngredientModel>();
        }));

        public static IngredientModel ToIngredientModel(this RecipeIngredient item)
        {
            var result = _mapper.Map<IngredientModel>(item.Ingredient);
            result.RecipeId = item.RecipeId;
            result.IngredientId = item.IngredientId; 
            result.Quantity = item.Quantity;
            return result;
        }
    }

    public static class RecipeToEditRecipeMapper
    {
        private static IMapper _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            //cfg.CreateMap<Recipe, EditRecipeModel>();
            cfg.CreateMap<Recipe, EditRecipeModel>()
                .ForMember(model => model.ImageContent, opt => opt.Ignore())
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom(data => data.RecipeIngredients.Select(item => item.ToIngredientModel())))
                .ReverseMap()
                .ForMember(model => model.RecipeIngredients, opt => opt.Ignore())
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }));

        public static EditRecipeModel ToEditRecipeModel(this Recipe item)
        {
            return _mapper.Map<EditRecipeModel>(item);
        }
    }
}
