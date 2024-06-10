using AutoMapper;
using Common.Data.Entities;
using Common.Models;

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
