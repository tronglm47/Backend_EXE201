using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.PostType;
using Services.RequestsResponses.PropertyForm;
using Services.RequestsResponses.PropertyType;

namespace Services.RequestsResponses.Post
{
    public class PostResponse
    {
        public class PostGetAllResponse
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public int PostTypeId { get; set; }
            public int PropertyTypeId { get; set; }
            public int PropertyFormId { get; set; }
            public int LocationId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Images { get; set; }
            public decimal? Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            public int? Views { get; set; }
        }
        public class PostGetByIdResponse
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public int PostTypeId { get; set; }
            public PostTypeResponse.PostTypeGetById PostType {  get; set; }
            public int PropertyTypeId { get; set; }
            public PropertyTypeResponse.PropertyTypeGetByIdResponse PropertyType {  get; set; }
            public int PropertyFormId { get; set; }
            public PropertyFormResponse.PropertyFormGetByIdResponse PropertyForm { get; set; }
            public int LocationId { get; set; }
            public List<int> AmenitiesId { get; set; }
            public List<AmenityResponse.GetByIdResponse> Amenities { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Images { get; set; }
            public decimal? Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            public int? Views { get; set; }
        }
    }
}