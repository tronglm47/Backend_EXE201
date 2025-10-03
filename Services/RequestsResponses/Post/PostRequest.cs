using System.ComponentModel.DataAnnotations;
using Services.RequestsResponses.Apartment;

namespace Services.RequestsResponses.Post
{
    public class PostRequest
    {
        public class PostCreateForUser
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
        }
        public class PostUpdateForUser
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
        }
        public class PostCreateForLandLord
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public string status { get; set; }
            public List<int>? UtilityIds { get; set; }
            public ApartmentRequest.ApartmentCreate Apartment { get; set; }
        }
        public class PostUpdateForLandLord
        {
            [Required]
            public string Title { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public string status { get; set; }
            public List<int>? UtilityIds { get; set; }
            public ApartmentRequest.ApartmentUpdate Apartment { get; set; }
        }
    }
}
