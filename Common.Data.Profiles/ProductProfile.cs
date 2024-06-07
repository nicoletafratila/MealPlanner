using AutoMapper;
using Common.Data.Entities;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>()
               .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
               .ReverseMap()
               .ForMember(data => data.ImageContent, opt => opt.Ignore())
               .ForMember(data => data.BaseUnit, opt => opt.Ignore())
               .ForMember(data => data.ProductCategory, opt => opt.Ignore());

            CreateMap<Product, ProductEditModel>()
                .ForMember(model => model.ImageUrl, opt => opt.MapFrom(data => string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(data.ImageContent!))))
                .ReverseMap()
                .ForMember(data => data.BaseUnit, opt => opt.Ignore())
                .ForMember(data => data.ProductCategory, opt => opt.Ignore());
        }
    }
}
