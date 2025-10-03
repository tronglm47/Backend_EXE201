using AutoMapper;
using Services.RequestsResponses.Subdivision;
using Repositories.Models;
using Services.RequestsResponses.Utility;
using Services.RequestsResponses.PostUtility;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.Apartment;

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
            CreateMap<ApartmentRequest.ApartmentCreate, Apartment>();
            CreateMap<ApartmentRequest.ApartmentUpdate, Apartment>();
            
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
           // Utility
            CreateMap<UtilityResponse.UtilityGetAll, Utility>().ReverseMap();
            CreateMap<UtilityResponse.UtilityDetail, Utility>().ReverseMap();
            CreateMap<UtilityRequest.UtilityCreate, Utility>();
            CreateMap<UtilityRequest.UtilityUpdate, Utility>();
        }
    }
}
