using AutoMapper;
using Common.Data.Profiles;
using Common.Models;
using RecipeBook.Data.Entities;
using RecipeBook.Shared.Models;

namespace RecipeBook.Data.Profiles
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Job, JobModel>()
                .IgnoreBaseModelMembers()
                .ForMember(
                    model => model.PercentComplete,
                    opt => opt.MapFrom(data => data.TotalCount <= 0 ? 0 : (int)(data.ProcessedCount * 100.0 / data.TotalCount)));
        }
    }
}
