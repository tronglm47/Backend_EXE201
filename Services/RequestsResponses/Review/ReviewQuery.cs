using Services.RequestsResponses;
using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Review
{
    public class ReviewQuery : QueryParametersBase
    {
        public override string DefaultSortBy => "createdAt";
        public override string DefaultSearchField => "description";

        [Range(1, int.MaxValue, ErrorMessage = "PostID must be greater than 0")]
        public int? PostId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "UserID must be greater than 0")]  
        public int? UserId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }
    }
}