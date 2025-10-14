using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Repositories.Constants;
using Services.RequestsResponses;
using Services.RequestsResponses.Review;
using Services.Utils;
using static Services.Utils.NullValueHandler;

namespace Services
{
    public interface IReviewService
    {
        Task<PagedResponse<object>> GetReviewsByPostAsync(int postId, ReviewQuery queryParams);
        Task<PagedResponse<object>> GetMyReviewsAsync(int userId, ReviewQuery queryParams);
        Task<object?> GetByIdAsync(int reviewId, List<string>? selectedFields = null);
        Task<ReviewResponse.ReviewDetail?> GetDetailAsync(int reviewId);
        Task<int> CreateReviewAsync(ReviewRequest.ReviewCreate request, int userId);
        Task<bool> UpdateReviewAsync(int reviewId, ReviewRequest.ReviewUpdate request, int userId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<bool> CanUserReviewAsync(int userId, int bookingId);
    }

    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;
        private readonly ReviewField _fieldResponse;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fieldResponse = new ReviewField();
        }

        /// <summary>
        /// Handle null values and set defaults
        /// </summary>
        private ReviewResponse.ReviewUserInfo? CreateUserInfo(User? user)
        {
            if (user == null) return null;
            
            return new ReviewResponse.ReviewUserInfo
            {
                UserId = user.UserId,
                Username = UserHandler.GetUsername(user),
                FullName = UserHandler.GetFullName(user),
                ProfilePictureUrl = UserHandler.GetProfilePictureUrl(user)
            };
        }

        private ReviewResponse.ReviewApartmentInfo? CreateApartmentInfo(Apartment? apartment)
        {
            if (apartment == null) return null;
            
            return new ReviewResponse.ReviewApartmentInfo
            {
                ApartmentId = apartment.ApartmentId,
                ApartmentCode = ApartmentHandler.GetApartmentCode(apartment),
                Floor = ApartmentHandler.GetFloor(apartment),
                Area = ApartmentHandler.GetArea(apartment),
                Building = apartment.Building != null ? new ReviewResponse.ReviewBuildingInfo
                {
                    BuildingId = apartment.Building.BuildingId,
                    Name = BuildingHandler.GetName(apartment.Building),
                    BlockCode = BuildingHandler.GetBlockCode(apartment.Building)
                } : null
            };
        }

        private ReviewResponse.PostInfo? CreatePostInfo(Post? post)
        {
            if (post == null) return null;
            
            return new ReviewResponse.PostInfo
            {
                PostId = post.PostId,
                Title = PostHandler.GetTitle(post),
                Description = PostHandler.GetDescription(post),
                Price = PostHandler.GetPrice(post),
                AverageRating = PostHandler.GetAverageRating(post),
                TotalReviews = PostHandler.GetTotalReviews(post),
                Apartment = CreateApartmentInfo(post.Apartment)
            };
        }

        private ReviewResponse.ReviewBookingInfo? CreateBookingInfo(Booking? booking)
        {
            if (booking == null) return null;
            
            return new ReviewResponse.ReviewBookingInfo
            {
                BookingId = booking.BookingId,
                MeetingTime = booking.MeetingTime,
                PlaceMeet = BookingHandler.GetPlaceMeet(booking),
                Status = BookingHandler.GetStatus(booking),
                CreatedAt = booking.CreatedAt
            };
        }

        /// <summary>
        /// Get all reviews for a specific post with pagination
        /// </summary>
        public async Task<PagedResponse<object>> GetReviewsByPostAsync(int postId, ReviewQuery queryParams)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetReviewsByPostIdAsync(
                    postId,
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.SortBy ?? "createdAt",
                    queryParams.IsDescending);

                var totalItems = await _unitOfWork.Reviews.CountReviewsByPostIdAsync(postId);

                var selectedFields = !string.IsNullOrWhiteSpace(queryParams.Select)
                    ? queryParams.Select.Split(',').Select(f => f.Trim()).ToList()
                    : new List<string>();

                var data = reviews.Select(review =>
                {
                    if (selectedFields.Any())
                    {
                        return _fieldResponse.SelectFields(review, selectedFields);
                    }

                    return new ReviewResponse.ReviewInfo
                    {
                        ReviewId = review.ReviewId,
                        BookingId = review.BookingId,
                        UserId = review.UserId,
                        PostId = review.PostId,
                        Rating = review.Rating,
                        Description = ReviewHandler.GetDescription(review),
                        CreatedAt = review.CreatedAt?.ToVietnamTime(),
                        UpdatedAt = review.UpdatedAt?.ToVietnamTime(),
                        User = review.User != null ? new ReviewResponse.ReviewUserInfo
                        {
                            UserId = review.User.UserId,
                            Username = review.User.Username,
                            FullName = review.User.FullName,
                            ProfilePictureUrl = review.User.ProfilePictureUrl
                        } : null
                    };
                });

                return new PagedResponse<object>
                {
                    Items = data.ToList(),
                    CurrentPage = queryParams.Page,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting reviews for post {PostId}", postId);
                throw;
            }
        }

        /// <summary>
        /// Get all reviews by current user with pagination
        /// </summary>
        public async Task<PagedResponse<object>> GetMyReviewsAsync(int userId, ReviewQuery queryParams)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetReviewsByUserIdAsync(
                    userId,
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.SortBy ?? "createdAt",
                    queryParams.IsDescending);

                var totalItems = await _unitOfWork.Reviews.CountReviewsByUserIdAsync(userId);

                var selectedFields = !string.IsNullOrWhiteSpace(queryParams.Select)
                    ? queryParams.Select.Split(',').Select(f => f.Trim()).ToList()
                    : new List<string>();

                var data = reviews.Select(review =>
                {
                    if (selectedFields.Any())
                    {
                        return _fieldResponse.SelectFields(review, selectedFields);
                    }

                    return new ReviewResponse.ReviewInfo
                    {
                        ReviewId = review.ReviewId,
                        BookingId = review.BookingId,
                        UserId = review.UserId,
                        PostId = review.PostId,
                        Rating = review.Rating,
                        Description = ReviewHandler.GetDescription(review),
                        CreatedAt = review.CreatedAt?.ToVietnamTime(),
                        UpdatedAt = review.UpdatedAt?.ToVietnamTime(),
                        User = review.User != null ? new ReviewResponse.ReviewUserInfo
                        {
                            UserId = review.User.UserId,
                            Username = review.User.Username,
                            FullName = review.User.FullName,
                            ProfilePictureUrl = review.User.ProfilePictureUrl
                        } : null
                    };
                });

                return new PagedResponse<object>
                {
                    Items = data.ToList(),
                    CurrentPage = queryParams.Page,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting reviews for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get review by ID with dynamic field selection
        /// </summary>
        public async Task<object?> GetByIdAsync(int reviewId, List<string>? selectedFields = null)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdWithDetailsAsync(reviewId);
                if (review == null)
                    return null;

                if (selectedFields?.Any() == true)
                {
                    return _fieldResponse.SelectFields(review, selectedFields);
                }

                return new ReviewResponse.ReviewDetail
                {
                    ReviewId = review.ReviewId,
                    BookingId = review.BookingId,
                    UserId = review.UserId,
                    PostId = review.PostId,
                    Rating = review.Rating,
                    Description = ReviewHandler.GetDescription(review),
                    CreatedAt = review.CreatedAt?.ToVietnamTime(),
                    UpdatedAt = review.UpdatedAt?.ToVietnamTime(),
                    User = review.User != null ? new ReviewResponse.ReviewUserInfo
                    {
                        UserId = review.User.UserId,
                        Username = review.User.Username,
                        FullName = review.User.FullName,
                        ProfilePictureUrl = review.User.ProfilePictureUrl
                    } : null,
                    Post = review.Post != null ? new ReviewResponse.PostInfo
                    {
                        PostId = review.Post.PostId,
                        Title = review.Post.Title,
                        Description = review.Post.Description,
                        Price = review.Post.Price,
                        AverageRating = review.Post.AverageRating,
                        TotalReviews = review.Post.TotalReviews,
                        Apartment = review.Post.Apartment != null ? new ReviewResponse.ReviewApartmentInfo
                        {
                            ApartmentId = review.Post.Apartment.ApartmentId,
                            ApartmentCode = review.Post.Apartment.ApartmentCode,
                            Floor = review.Post.Apartment.Floor,
                            Area = review.Post.Apartment.Area,
                            Building = review.Post.Apartment.Building != null ? new ReviewResponse.ReviewBuildingInfo
                            {
                                BuildingId = review.Post.Apartment.Building.BuildingId,
                                Name = review.Post.Apartment.Building.Name,
                                BlockCode = review.Post.Apartment.Building.BlockCode
                            } : null
                        } : null
                    } : null,
                    Booking = review.Booking != null ? new ReviewResponse.ReviewBookingInfo
                    {
                        BookingId = review.Booking.BookingId,
                        MeetingTime = review.Booking.MeetingTime?.ToVietnamTime(),
                        PlaceMeet = review.Booking.PlaceMeet,
                        Status = review.Booking.Status,
                        CreatedAt = review.Booking.CreatedAt?.ToVietnamTime()
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting review {ReviewId}", reviewId);
                throw;
            }
        }

        /// <summary>
        /// Get review detail by ID
        /// </summary>
        public async Task<ReviewResponse.ReviewDetail?> GetDetailAsync(int reviewId)
        {
            try
            {
                var result = await GetByIdAsync(reviewId);
                return result as ReviewResponse.ReviewDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting review detail {ReviewId}", reviewId);
                throw;
            }
        }

        /// <summary>
        /// Create a new review
        /// </summary>
        public async Task<int> CreateReviewAsync(ReviewRequest.ReviewCreate request, int userId)
        {
            try
            {
                // Validate user can create review for this booking
                var canReview = await _unitOfWork.Reviews.CheckUserCanReviewAsync(userId, request.BookingId);
                if (!canReview)
                {
                    throw new UnauthorizedAccessException("You are not authorized to review this booking or booking is not eligible for review");
                }

                // Get booking to get post ID
                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    throw new ArgumentException("Booking not found");
                }

                var review = new Review
                {
                    BookingId = request.BookingId,
                    UserId = userId,
                    PostId = booking.PostId,
                    Rating = request.Rating,
                    Description = request.Description,
                    CreatedAt = TimeZoneConverter.GetVietnamCurrentTime(),
                    UpdatedAt = TimeZoneConverter.GetVietnamCurrentTime(),
                    IsDeleted = false
                };

                await _unitOfWork.Reviews.CreateAsync(review);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Review created successfully for booking {BookingId} by user {UserId}", request.BookingId, userId);
                return review.ReviewId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating review for booking {BookingId}", request.BookingId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing review
        /// </summary>
        public async Task<bool> UpdateReviewAsync(int reviewId, ReviewRequest.ReviewUpdate request, int userId)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
                if (review == null)
                {
                    throw new ArgumentException("Review not found");
                }

                // Check if user owns this review
                if (review.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this review");
                }

                // Check if user has correct role
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user?.Role?.ToLower() != UserRoleConstants.UserRole)
                {
                    throw new UnauthorizedAccessException("Only users with 'user' role can update reviews");
                }

                review.Rating = request.Rating;
                review.Description = request.Description;
                review.UpdatedAt = TimeZoneConverter.GetVietnamCurrentTime();

                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Review {ReviewId} updated successfully by user {UserId}", reviewId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating review {ReviewId}", reviewId);
                throw;
            }
        }

        /// <summary>
        /// Delete a review (soft delete)
        /// </summary>
        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
                if (review == null)
                {
                    throw new ArgumentException("Review not found");
                }

                // Check if user owns this review
                if (review.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this review");
                }

                // Soft delete
                review.IsDeleted = true;
                review.UpdatedAt = TimeZoneConverter.GetVietnamCurrentTime();

                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Review {ReviewId} deleted successfully by user {UserId}", reviewId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting review {ReviewId}", reviewId);
                throw;
            }
        }

        /// <summary>
        /// Check if user can review a booking
        /// </summary>
        public async Task<bool> CanUserReviewAsync(int userId, int bookingId)
        {
            try
            {
                return await _unitOfWork.Reviews.CheckUserCanReviewAsync(userId, bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if user {UserId} can review booking {BookingId}", userId, bookingId);
                throw;
            }
        }
    }
}
