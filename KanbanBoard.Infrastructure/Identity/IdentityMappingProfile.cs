using AutoMapper;
using KanbanBoard.Application.DTOs.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Infrastructure.Identity
{
    public class IdentityMappingProfile : Profile
    {
        public IdentityMappingProfile()
        {
            CreateMap<RegisterDto, AppUserIdentity>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<AppUserIdentity, AuthUserDto>();
        }
    }
}
