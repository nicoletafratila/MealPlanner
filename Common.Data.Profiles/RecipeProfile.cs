﻿using AutoMapper;
using Common.Data.Entities;
using Common.Data.Profiles.Resolvers;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, RecipeModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
                .ForMember(model => model.RecipeCategoryName, opt => opt.MapFrom(data => data.RecipeCategory!.Name))
                .ForMember(model => model.RecipeCategoryId, opt => opt.MapFrom(data => data.RecipeCategory!.Id))
                .ReverseMap();

            CreateMap<Recipe, RecipeEditModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
                .ForMember(model => model.Ingredients, opt => opt.MapFrom<RecipeToEditRecipeModelResolver, IList<RecipeIngredient>?>(data => data.RecipeIngredients!))
                .ReverseMap()
                .ForMember(data => data.RecipeCategory, opt => opt.Ignore())
                .ForMember(data => data.RecipeIngredients, opt => opt.MapFrom<EditRecipeModelToRecipeResolver, IList<RecipeIngredientEditModel>?>(model => model.Ingredients!));
        }
    }
}
