using AutoMapper;
using Services.RequestsResponses.Subdivision;
using Services.RequestsResponses.Building;
using Repositories.Models;
using Services.RequestsResponses.Utility;
using Services.RequestsResponses.PostUtility;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.Apartment;
using Services.RequestsResponses.User;
using Services.RequestsResponses.Review;

namespace Services.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Post
            CreateMap<PostResponse.PostGetAll, Post>().ReverseMap();
            CreateMap<PostRequest.PostCreateForUser, Post>();
            CreateMap<PostRequest.PostCreateForLandLord, Post>();
            
            // Apartment
            CreateMap<ApartmentResponse.ApartmentGetAll, Apartment>()
                .ForMember(dest => dest.Building, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId ?? 0))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.Floor ?? 0))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => (double)(src.Area ?? 0)))
                .ForMember(dest => dest.NumberOfBedrooms, opt => opt.MapFrom(src => src.NumberBathroom ?? 0))
                .ForMember(dest => dest.PostIds, opt => opt.MapFrom(src => src.Posts.Select(p => p.PostId).ToList()));
            CreateMap<ApartmentResponse.ApartmentDetail, Apartment>().ReverseMap()
                .ForMember(dest => dest.NumberOfBedrooms, opt => opt.MapFrom(src => src.NumberBathroom ?? 0))
                .ForMember(dest => dest.Floor, opt => opt.MapFrom(src => src.Floor ?? 0))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => (double)(src.Area ?? 0)));
            CreateMap<ApartmentRequest.ApartmentCreate, Apartment>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => (decimal)src.Area))
                .ForMember(dest => dest.NumberBathroom, opt => opt.MapFrom(src => src.NumberBathroom));
            CreateMap<ApartmentRequest.ApartmentUpdate, Apartment>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => (decimal)src.Area));
            
            // PostUtility
            CreateMap<PostUtilityResponse.PostUtilityGetAll, PostUtility>().ReverseMap();
            CreateMap<PostUtilityResponse.PostUtilityDetail, PostUtility>().ReverseMap();
            CreateMap<PostUtilityRequest.PostUtilityCreate, PostUtility>();
            CreateMap<PostUtilityRequest.PostUtilityUpdate, PostUtility>();
            
            // Subdivision
            CreateMap<SubdivisionResponse.SubdivisionGetAll, Subdivision>().ReverseMap();
            CreateMap<SubdivisionResponse.SubdivisionDetail, Subdivision>().ReverseMap();
            CreateMap<SubdivisionRequest.SubdivisionCreate, Subdivision>();
            CreateMap<SubdivisionRequest.SubdivisionUpdate, Subdivision>();
            
            // Building
            CreateMap<BuildingResponse.BuildingGetAll, Building>()
                .ForMember(dest => dest.Subdivision, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.SubdivisionName, opt => opt.MapFrom(src => src.Subdivision != null ? src.Subdivision.Name : null));
            CreateMap<BuildingResponse.BuildingDetail, Building>().ReverseMap();
            CreateMap<BuildingRequest.BuildingCreate, Building>();
            CreateMap<BuildingRequest.BuildingUpdate, Building>();
            
            // Utility
            CreateMap<UtilityResponse.UtilityGetAll, Utility>().ReverseMap();
            CreateMap<UtilityResponse.UtilityDetail, Utility>().ReverseMap();
            CreateMap<UtilityRequest.UtilityCreate, Utility>();
            CreateMap<UtilityRequest.UtilityUpdate, Utility>();

            // User
            CreateMap<UserResponse.UserGetAll, User>().ReverseMap();
            CreateMap<UserResponse.UserDetail, User>().ReverseMap();
            CreateMap<UserRequest.AdminUpdateUserRequest, User>();

            // Review
            CreateMap<Review, ReviewResponse.ReviewInfo>().ReverseMap();
            CreateMap<Review, ReviewResponse.ReviewDetail>().ReverseMap();
            CreateMap<ReviewRequest.ReviewCreate, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            CreateMap<ReviewRequest.ReviewUpdate, Review>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
