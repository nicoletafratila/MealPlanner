using AutoMapper;
using Common.Data.Profiles;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

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
