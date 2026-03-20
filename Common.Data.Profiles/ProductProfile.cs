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
                .IgnoreBaseModelMembers()
                .ForMember(
                    m => m.ImageUrl,
                    o => o.MapFrom(src =>
                        $"data:image/jpg;base64,{Convert.ToBase64String(src.ImageContent ?? Array.Empty<byte>())}")
                )
                .ReverseMap()
                .ForMember(d => d.ImageContent, o => o.Ignore())
                .ForMember(d => d.BaseUnit, o => o.Ignore())
                .ForMember(d => d.ProductCategory, o => o.Ignore());

            CreateMap<Product, ProductEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    m => m.ImageUrl,
                    o => o.MapFrom(src =>
                        $"data:image/jpg;base64,{Convert.ToBase64String(src.ImageContent ?? Array.Empty<byte>())}")
                )
                .ReverseMap()
                .ForMember(d => d.BaseUnit, o => o.Ignore())
                .ForMember(d => d.ProductCategory, o => o.Ignore());
        }
    }
}