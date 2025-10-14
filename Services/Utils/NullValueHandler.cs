using Repositories.Models;

namespace Services.Utils
{
    /// <summary>
    /// Utility class to handle null values and set appropriate defaults
    /// </summary>
    public static class NullValueHandler
    {
        /// <summary>
        /// Handle null string values - return empty string instead of null
        /// </summary>
        public static string HandleString(string? value)
        {
            return value ?? string.Empty;
        }

        /// <summary>
        /// Handle nullable int values - return 0 instead of null
        /// </summary>
        public static int HandleInt(int? value)
        {
            return value ?? 0;
        }

        /// <summary>
        /// Handle nullable decimal values - return 0 instead of null
        /// </summary>
        public static decimal HandleDecimal(decimal? value)
        {
            return value ?? 0;
        }

        /// <summary>
        /// Handle nullable double values - return 0 instead of null
        /// </summary>
        public static double HandleDouble(double? value)
        {
            return value ?? 0;
        }

        /// <summary>
        /// Handle nullable bool values - return false instead of null
        /// </summary>
        public static bool HandleBool(bool? value)
        {
            return value ?? false;
        }

        /// <summary>
        /// Handle nullable DateTime values - keep null for optional dates
        /// </summary>
        public static DateTime? HandleDateTime(DateTime? value)
        {
            return value; // Keep null for optional dates
        }

        /// <summary>
        /// Handle User object and return safe values
        /// </summary>
        public static class UserHandler
        {
            public static string GetUsername(User? user)
            {
                return user?.Username ?? string.Empty;
            }

            public static string GetFullName(User? user)
            {
                return user?.FullName ?? string.Empty;
            }

            public static string GetEmail(User? user)
            {
                return user?.Email ?? string.Empty;
            }

            public static string GetPhoneNumber(User? user)
            {
                return user?.PhoneNumber ?? string.Empty;
            }

            public static string GetProfilePictureUrl(User? user)
            {
                return user?.ProfilePictureUrl ?? string.Empty;
            }

            public static string GetBio(User? user)
            {
                return user?.Bio ?? string.Empty;
            }

            public static string GetRole(User? user)
            {
                return user?.Role ?? string.Empty;
            }

            public static bool GetIsEmailVerified(User? user)
            {
                return user?.IsEmailVerified ?? false;
            }

            public static bool GetIsLocationSharingEnabled(User? user)
            {
                return user?.IsLocationSharingEnabled ?? false;
            }
        }

        /// <summary>
        /// Handle Post object and return safe values
        /// </summary>
        public static class PostHandler
        {
            public static string GetTitle(Post? post)
            {
                return post?.Title ?? string.Empty;
            }

            public static string GetDescription(Post? post)
            {
                return post?.Description ?? string.Empty;
            }

            public static decimal GetPrice(Post? post)
            {
                return post?.Price ?? 0;
            }

            public static string GetPostType(Post? post)
            {
                return post?.PostType ?? string.Empty;
            }

            public static string GetStatus(Post? post)
            {
                return post?.Status ?? string.Empty;
            }

            public static decimal GetAverageRating(Post? post)
            {
                return post?.AverageRating ?? 0;
            }

            public static int GetTotalReviews(Post? post)
            {
                return post?.TotalReviews ?? 0;
            }
        }

        /// <summary>
        /// Handle Apartment object and return safe values
        /// </summary>
        public static class ApartmentHandler
        {
            public static string GetApartmentCode(Apartment? apartment)
            {
                return apartment?.ApartmentCode ?? string.Empty;
            }

            public static string GetApartmentType(Apartment? apartment)
            {
                return apartment?.ApartmentType ?? string.Empty;
            }

            public static string GetStatus(Apartment? apartment)
            {
                return apartment?.Status ?? string.Empty;
            }

            public static int GetFloor(Apartment? apartment)
            {
                return apartment?.Floor ?? 0;
            }

            public static decimal GetArea(Apartment? apartment)
            {
                return apartment?.Area ?? 0;
            }



            public static int GetNumberBathroom(Apartment? apartment)
            {
                return apartment?.NumberBathroom ?? 0;
            }
        }

        /// <summary>
        /// Handle Building object and return safe values
        /// </summary>
        public static class BuildingHandler
        {
            public static string GetName(Building? building)
            {
                return building?.Name ?? string.Empty;
            }

            public static string GetBlockCode(Building? building)
            {
                return building?.BlockCode ?? string.Empty;
            }


        }

        /// <summary>
        /// Handle Booking object and return safe values
        /// </summary>
        public static class BookingHandler
        {
            public static string GetPlaceMeet(Booking? booking)
            {
                return booking?.PlaceMeet ?? string.Empty;
            }

            public static string GetStatus(Booking? booking)
            {
                return booking?.Status ?? string.Empty;
            }

            public static string GetMeetingAddress(Booking? booking)
            {
                return booking?.MeetingAddress ?? string.Empty;
            }

            public static bool GetIsLocationTrackingEnabled(Booking? booking)
            {
                return booking?.IsLocationTrackingEnabled ?? false;
            }

            public static decimal GetMeetingLatitude(Booking? booking)
            {
                return booking?.MeetingLatitude ?? 0;
            }

            public static decimal GetMeetingLongitude(Booking? booking)
            {
                return booking?.MeetingLongitude ?? 0;
            }

            public static decimal GetDistanceToMeeting(Booking? booking)
            {
                return booking?.DistanceToMeeting ?? 0;
            }
        }

        /// <summary>
        /// Handle Review object and return safe values
        /// </summary>
        public static class ReviewHandler
        {
            public static string GetDescription(Review? review)
            {
                return review?.Description ?? string.Empty;
            }

            public static bool GetIsDeleted(Review? review)
            {
                return review?.IsDeleted ?? false;
            }
        }
    }
}