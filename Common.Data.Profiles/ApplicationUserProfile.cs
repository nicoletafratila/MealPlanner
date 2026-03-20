using AutoMapper;
using Common.Data.Entities;
using Identity.Shared.Models;

namespace Common.Data.Profiles
{
    public class ApplicationUserProfile : Profile
    {
        public ApplicationUserProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserEditModel>()
                .IgnoreBaseModelMembers()
                .ForMember(m => m.UserId, o => o.MapFrom(s => s.Id))
                .ForMember(m => m.Username, o => o.MapFrom(s => s.UserName))
                .ForMember(m => m.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(m => m.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(
                    m => m.ProfilePictureUrl,
                    o => o.MapFrom(s =>
                        $"data:image/jpg;base64,{Convert.ToBase64String(s.ProfilePicture ?? Array.Empty<byte>())}"
                    ))
                .ForMember(m => m.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
                .ForMember(m => m.EmailAddress, o => o.MapFrom(s => s.Email))
                .ForMember(m => m.IsActive, o => o.MapFrom(s => s.IsActive))
                .ReverseMap();
        }
    }
}