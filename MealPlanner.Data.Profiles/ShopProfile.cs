using AutoMapper;
using Common.Data.Profiles;
using Common.Models;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles
{
    public class ShopProfile : Profile
    {
        public ShopProfile()
        {
            CreateMap<Shop, ShopModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    model => model.ImageUrl,
                    opt => opt.MapFrom(data =>
                        data.ImageContent == null || data.ImageContent.Length == 0 ? null : $"data:image/jpg;base64,{Convert.ToBase64String(data.ImageContent)}"
                    ))
                .ForMember(
                    model => model.ThumbnailUrl,
                    opt => opt.MapFrom(data =>
                        data.ImageThumbnail == null || data.ImageThumbnail.Length == 0 ? null : $"data:image/jpg;base64,{Convert.ToBase64String(data.ImageThumbnail)}"
                    ))
                .ReverseMap()
                .ForMember(dest => dest.DisplaySequence, opt => opt.Ignore())
                .ForMember(dest => dest.ImageContent, opt => opt.Ignore())
                .ForMember(dest => dest.ImageThumbnail, opt => opt.Ignore());

            CreateMap<Shop, ShopEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    model => model.ImageUrl,
                    opt => opt.MapFrom(data =>
                        data.ImageContent == null || data.ImageContent.Length == 0 ? null : $"data:image/jpg;base64,{Convert.ToBase64String(data.ImageContent)}"
                    ))
                .ForMember(
                    dest => dest.DisplaySequence,
                    opt => opt.MapFrom(src => src.DisplaySequence == null
                        ? new List<ShopDisplaySequence>()
                        : src.DisplaySequence.OrderBy(s => s.Value).ToList())
                )
                .AfterMap((src, dest) => dest.DisplaySequence?.SetIndexes())
                .ReverseMap()
                .ForMember(dest => dest.DisplaySequence, opt => opt.MapFrom(src => src.DisplaySequence));
        }
    }
}
