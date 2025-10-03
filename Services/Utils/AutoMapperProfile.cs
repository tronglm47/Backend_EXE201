using AutoMapper;
using Services.RequestsResponses.Subdivision;
using VLivingAPI.Repositories.Models;

namespace Services.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
           CreateMap<SubdivisionResponse.SubdivisionGetAll, Subdivision>().ReverseMap();
           CreateMap<SubdivisionResponse.SubdivisionDetail, Subdivision>().ReverseMap();
           CreateMap<SubdivisionRequest.SubdivisionCreate, Subdivision>();
           CreateMap<SubdivisionRequest.SubdivisionUpdate, Subdivision>();
        }
    }
}
