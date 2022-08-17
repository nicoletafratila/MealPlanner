using AutoMapper;
using RecipeBook.Api.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Data.Profiles
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, EditRecipeModel>()
                .ForMember(model => model.ImageContent, opt => opt.Ignore())
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom((data, _, transactionCode, ctx) =>
                {
                    var result = new List<IngredientModel>();
                    foreach (var item in data.RecipeIngredients)
                    {
                        var model = ctx.Mapper.Map<IngredientModel>(item.Ingredient);
                        model.RecipeId = item.RecipeId;
                        model.IngredientId = item.IngredientId;
                        model.Quantity = item.Quantity;
                        result.Add(model);
                    }
                    return result;
                }))
                .ReverseMap()
                .ForMember(model => model.RecipeIngredients, opt => opt.Ignore())
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());

            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent))))
                .ReverseMap()
                .ForMember(model => model.RecipeIngredients, opt => opt.Ignore())
                .ForMember(model => model.MealPlanRecipes, opt => opt.Ignore());
        }
    }
}
