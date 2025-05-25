using Application.DTOs;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Profiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TaskItem, TaskDto>(); 
            CreateMap<CreateTaskDto, TaskItem>();
            CreateMap<RegisterDto, IdentityUser>();
            CreateMap<IdentityUser, AuthDto>();
        }
    }
}
