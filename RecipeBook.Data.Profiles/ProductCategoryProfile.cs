using AutoMapper;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;
using Common.Data.Profiles;

namespace RecipeBook.Data.Profiles
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            CreateMap<ProductCategory, ProductCategoryModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap();

            CreateMap<ProductCategory, ProductCategoryEditModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap();
        }
    }
}
