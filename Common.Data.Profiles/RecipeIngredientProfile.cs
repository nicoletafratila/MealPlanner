using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeIngredientProfile : Profile
    {
        public RecipeIngredientProfile()
        {
            CreateMap<RecipeIngredient, RecipeIngredientEditModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap()
                .ForMember(d => d.Recipe, o => o.Ignore())
                .ForMember(d => d.Product, o => o.Ignore())
                .ForMember(d => d.Unit, o => o.Ignore());

            CreateMap<RecipeIngredient, ShoppingListProductEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(d => d.ShoppingListId, o => o.Ignore())
                .ForMember(d => d.Collected, o => o.Ignore())
                .ForMember(d => d.DisplaySequence, o => o.Ignore())
                .ReverseMap()
                .ForMember(d => d.Recipe, o => o.Ignore())
                .ForMember(d => d.Product, o => o.Ignore())
                .ForMember(d => d.Unit, o => o.Ignore());
        }
    }
}