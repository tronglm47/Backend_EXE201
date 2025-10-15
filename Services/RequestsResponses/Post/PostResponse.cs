using Services.RequestsResponses.Apartment;
using Services.RequestsResponses.Utility;

namespace Services.RequestsResponses.Post
{
    public class PostResponse
    {
        public class PostImageInfo
        {
            public int ImageId { get; set; }
            public string ImageUrl { get; set; } = null!;
            public int? DisplayOrder { get; set; }
            public bool? IsPrimary { get; set; }
        }

        public class PostUtilityInfo
        {
            public int UtilityId { get; set; }
            public string Name { get; set; } = null!;
            public string? Notes { get; set; }
        }

        public class PostGetAll
        {
            public int PostId { get; set; }
            public int? ApartmentId { get; set; }
            public int? UserId { get; set; }
            public string? UserName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Email { get; set; }
            public string? FullName { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public double? Price { get; set; }
            public string PostType { get; set; } = null!;
            public string Status { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public List<PostImageInfo> Images { get; set; } = new List<PostImageInfo>();
            public List<PostUtilityInfo> Utilities { get; set; } = new List<PostUtilityInfo>();
        }
        public class PostGetAllForUser
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
        }
        public class PostDetailForUser
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
        }
        public class PostGetAllForLandLord
        {
            public int PostId { get; set; }
            public int? ApartmentId { get; set; }
            public string ApartmentCode { get; set; } = null!;
            public int Floor { get; set; }
            public double Area { get; set; }
            public int NumberBathroom { get; set; }
            public int BuildingId { get; set; }
            public string BuildingName { get; set; } = null!;
            public string BlockCode { get; set; } = null!;
            public string SubdivisionId { get; set; } = null!;
            public string SubdivisionName { get; set; } = null!;
            public int? UserId { get; set; }
            public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public double? Price { get; set; }
            public string PostType { get; set; } = null!;
            public string Status { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public List<PostImageInfo> Images { get; set; } = new List<PostImageInfo>();
            public List<PostUtilityInfo> Utilities { get; set; } = new List<PostUtilityInfo>();
        }
        public class PostDetailForLandLord
        {
            public int PostId { get; set; }
            public int? ApartmentId { get; set; }
            public ApartmentResponse.ApartmentDetail Apartment { get; set; } = null!;
            public int? UserId { get; set; }
            public string UserName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string Title { get; set; } = null!;
            public string Description { get; set; } = null!;
            public double? Price { get; set; }
            public string PostType { get; set; } = null!;
            public string Status { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public List<PostImageInfo> Images { get; set; } = new List<PostImageInfo>();
            public List<PostUtilityInfo> Utilities { get; set; } = new List<PostUtilityInfo>();
        }
    }
}
