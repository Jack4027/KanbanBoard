using AutoMapper;
using KanbanBoard.Application.DTOs.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Identity
{
    public class IdentityMappingProfile : Profile
    {
        //Mapping profile for AutoMapper to define how to map between the RegisterDto and AppUserIdentity, as well as between AppUserIdentity and AuthUserDto.
        public IdentityMappingProfile()
        {
            CreateMap<RegisterDto, AppUserIdentity>()
                //Map the Email property from RegisterDto to both the Email and UserName properties of AppUserIdentity, ensuring that the user's email is used as their username for authentication purposes.
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                //Ignore the PasswordHash property when mapping from RegisterDto to AppUserIdentity, as the password will be hashed and stored securely by ASP.NET Core Identity, and should not be directly mapped from the registration DTO.
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<AppUserIdentity, AuthUserDto>();
        }
    }
}
