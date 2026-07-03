using AutoMapper;
using Common.Data.Profiles;
using MealPlanner.Data.Entities;
using MealPlanner.Shared.Models;

namespace MealPlanner.Data.Profiles
{
    public class ShoppingListProfile : Profile
    {
        public ShoppingListProfile()
        {
            CreateMap<ShoppingList, ShoppingListModel>()
                .IgnoreBaseModelMembers()
                .ReverseMap()
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Shop, opt => opt.Ignore());

            CreateMap<ShoppingList, ShoppingListEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    dest => dest.Products,
                    opt => opt.MapFrom(src => src.Products == null || src.Products.Count == 0
                        ? new List<ShoppingListProduct>()
                        : src.Products
                            .OrderBy(i => i.Collected)
                            .ThenBy(i => i.DisplaySequence)
                            .ThenBy(i => i.Product == null ? null : i.Product.Name)
                            .ToList())
                )
                .ReverseMap()
                .ForMember(
                    dest => dest.Products,
                    opt => opt.MapFrom((src, dest, srcMember, context) =>
                        src.Products == null || src.Products.Count == 0
                            ? (IList<ShoppingListProduct>)[]
                            : src.Products
                                .Select(p =>
                                {
                                    var productId = p.Product?.Id ?? Guid.Empty;
                                    var existing = dest.Products?.FirstOrDefault(d => d.ProductId == productId);
                                    if (existing != null)
                                    {
                                        context.Mapper.Map(p, existing);
                                        return existing;
                                    }
                                    return context.Mapper.Map<ShoppingListProduct>(p);
                                })
                                .ToList()
                    )
                );
        }
    }
}
