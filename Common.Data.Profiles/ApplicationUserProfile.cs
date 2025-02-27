using AutoMapper;
using Common.Data.Entities;
using Identity.Shared.Models;

namespace Common.Data.Profiles
{
    public class ApplicationUserProfile : Profile
    {
        public ApplicationUserProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserModel>()
                .ForMember(model => model.UserId, opt => opt.MapFrom(data => data.Id))
                .ForMember(model => model.UserName, opt => opt.MapFrom(data => data.UserName))
                .ForMember(model => model.FirstName, opt => opt.MapFrom(data => data.FirstName))
                .ForMember(model => model.LastName, opt => opt.MapFrom(data => data.LastName))
                .ForMember(model => model.ProfilePictureUrl, opt => opt.MapFrom(data => $"data:image/jpg;base64,{Convert.ToBase64String(data!.ProfilePicture!)}"))
                .ForMember(model => model.PhoneNumber, opt => opt.MapFrom(data => data.PhoneNumber))
                .ForMember(model => model.EmailAddress, opt => opt.MapFrom(data => data.Email))
                .ForMember(model => model.IsActive, opt => opt.MapFrom(data => data.IsActive))
                .ReverseMap();
        }
    }
}
