using AutoMapper;
using BE_EXE201.Common.Payloads.Requests;
using BE_EXE201.Dtos;
using BE_EXE201.Entities;

namespace BE_EXE201.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper() 
        {
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<UserRole, UserRoleModel>().ReverseMap();
            CreateMap<Home, HomeModel>().ReverseMap();
            CreateMap<Image, ImageModel>().ReverseMap();
            CreateMap<Location, LocationModel>().ReverseMap();
            CreateMap<Report, ReportModel>().ReverseMap();
            CreateMap<Utilities, UtilitiesModel>().ReverseMap();
        }
    }
}
