using AutoMapper;
using Common.Data.Entities;
using Common.Shared.Models;

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
