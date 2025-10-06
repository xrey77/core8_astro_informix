using AutoMapper;
using core8_astro_informix.Entities;
using core8_astro_informix.Models.dto;

namespace core8_astro_informix.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserModel>();
            CreateMap<UserRegister, User>();
            CreateMap<UserLogin, User>();
            CreateMap<UserUpdate, User>();
            CreateMap<UserPasswordUpdate, User>();
            CreateMap<Product, ProductModel>();
             CreateMap<ProductModel, Product>();

        }
    }
    

}