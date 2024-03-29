using AutoMapper;
using Common.Data.Entities;
using Common.Shared;

namespace Common.Data.Profiles
{
    public class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<Log, LogModel>()
               .ReverseMap();
        }
    }
}
