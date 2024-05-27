using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            CreateMap<ProductCategory, ProductCategoryModel>()
               .ReverseMap();

            CreateMap<ProductCategory, EditProductCategoryModel>()
                .ReverseMap();
        }
    }
}
