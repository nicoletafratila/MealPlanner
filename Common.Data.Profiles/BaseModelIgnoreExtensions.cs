using AutoMapper;
using Common.Models;

namespace Common.Data.Profiles
{
    public static class BaseModelIgnoreExtensions
    {
        public static IMappingExpression<TSource, TDestination> IgnoreBaseModelMembers<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expr)
            where TDestination : BaseModel
        {
            return expr
                .ForMember(dest => dest.Index, opt => opt.Ignore())
                .ForMember(dest => dest.IsSelected, opt => opt.Ignore());
        }
    }

}
