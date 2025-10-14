using Services.RequestsResponses;
using Repositories.Models;

namespace Services.RequestsResponses.Review
{
    public class ReviewField : FieldResponseBase
    {
        protected override List<string> ValidFields => new()
        {
            "reviewid",
            "bookingid", 
            "userid",
            "postid",
            "rating",
            "description",
            "createdat",
            "updatedat",
            "user",
            "post",
            "booking"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            if (item is not Repositories.Models.Review review) return;

            switch (field)
            {
                case "reviewid":
                    result["reviewId"] = review.ReviewId;
                    break;
                case "bookingid":
                    result["bookingId"] = review.BookingId;
                    break;
                case "userid":
                    result["userId"] = review.UserId;
                    break;
                case "postid":
                    result["postId"] = review.PostId;
                    break;
                case "rating":
                    result["rating"] = review.Rating;
                    break;
                case "description":
                    result["description"] = review.Description;
                    break;
                case "createdat":
                    result["createdAt"] = review.CreatedAt;
                    break;
                case "updatedat":
                    result["updatedAt"] = review.UpdatedAt;
                    break;
                case "user":
                    result["user"] = review.User != null ? new
                    {
                        userId = review.User.UserId,
                        username = review.User.Username,
                        fullName = review.User.FullName,
                        profilePictureUrl = review.User.ProfilePictureUrl
                    } : null;
                    break;
                case "post":
                    result["post"] = review.Post != null ? new
                    {
                        postId = review.Post.PostId,
                        title = review.Post.Title,
                        averageRating = review.Post.AverageRating,
                        totalReviews = review.Post.TotalReviews
                    } : null;
                    break;
                case "booking":
                    result["booking"] = review.Booking != null ? new
                    {
                        bookingId = review.Booking.BookingId,
                        meetingTime = review.Booking.MeetingTime,
                        status = review.Booking.Status,
                        createdAt = review.Booking.CreatedAt
                    } : null;
                    break;
            }
        }
    }
}