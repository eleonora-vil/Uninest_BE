using AutoMapper;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Dtos;
using BE_EXE201.Entities;

namespace Mock_Project_Net03.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() 
        {
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserRole, UserRoleModel>().ReverseMap();
           
        }
    }
}
