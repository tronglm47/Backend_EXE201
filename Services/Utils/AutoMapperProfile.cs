using AutoMapper;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.PostAmenity;
using Services.RequestsResponses.PostType;
using Services.RequestsResponses.PropertyForm;
using Services.RequestsResponses.PropertyType;
using VLivingAPI.Repositories.Data.Models;

namespace Services.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PostResponse.PostGetAllResponse, Post>().ReverseMap();

            // Post
            CreateMap<PostRequest.CreatePost, Post>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
                .ForMember(dest => dest.Views, opt => opt.MapFrom(src => 0));

            // PostType
            CreateMap<PostTypeResponse.PostTypeGetById, PostType>().ReverseMap();
            CreateMap<PostTypeResponse.PostTypeGetAll, PostType>().ReverseMap();
            CreateMap<PostTypeRequest.PostTypeCreate, PostType>();
            CreateMap<PostTypeRequest.PostTypeUpdate, PostType>();
            // PostAmenity
            CreateMap<PostAmenityResponse.GetAll, PostAmenity>().ReverseMap();
            CreateMap<PostAmenityResponse.GetById, PostAmenity>().ReverseMap();
            CreateMap<PostAmenityRequest.CreatePostAmenity, PostAmenity>();
            CreateMap<PostAmenityRequest, PostAmenity>();
            // PropertyType
            CreateMap<PropertyTypeResponse.PropertyTypeGetAllResponse, PropertyType>().ReverseMap();
            CreateMap<PropertyTypeResponse.PropertyTypeGetByIdResponse, PropertyType>().ReverseMap();
            CreateMap<PropertyTypeRequest.CreateRequest, PropertyType>();
            CreateMap<PropertyTypeRequest.UpdateRequest, PropertyType>();
            // PropertyForm
            CreateMap<PropertyFormResponse.PropertyFormGetAllResponse, PropertyForm>().ReverseMap();
            CreateMap<PropertyFormResponse.PropertyFormGetByIdResponse, PropertyForm>().ReverseMap();
            CreateMap<PropertyFormRequest.CreatePropertyForm, PropertyForm>();
            CreateMap<PropertyFormRequest.UpdatePropertyForm, PropertyForm>();
            // Amenity
            CreateMap<AmenityResponse.GetALlResponse, Amenity>().ReverseMap();
            CreateMap<AmenityResponse.GetByIdResponse, Amenity>().ReverseMap();
            CreateMap<AmenityRequest.CreateAmenity, Amenity>();
            CreateMap<AmenityRequest.UpdateAmenity, Amenity>();
        }
    }
}
