using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Services.RequestsResponses;
using Services.RequestsResponses.Booking;
using Services.Utils;

namespace Services
{
    public interface IBookingService
    {
        Task<PagedResponse<object>> GetAllAsync(BookingQuery queryParams);
        Task<PagedResponse<object>> GetBookingsForRenterAsync(int renterId, BookingQuery queryParams);
        Task<PagedResponse<object>> GetBookingsForLandlordAsync(int landlordId, BookingQuery queryParams);
        Task<object?> GetByIdAsync(int bookingId, List<string>? selectedFields = null);
        Task<BookingResponse.BookingDetail?> GetDetailAsync(int bookingId);
        Task<int> CreateBookingAsync(BookingRequest.BookingCreate request, int renterId);
        Task<bool> UpdateBookingAsync(int bookingId, BookingRequest.BookingUpdate request, int userId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, BookingRequest.BookingStatusUpdate request, int userId);
        Task<bool> DeleteBookingAsync(int bookingId, int userId);
        Task<bool> CanCreateBookingAsync(int renterId, int postId);
    }

    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingService> _logger;
        private readonly BookingField _fieldResponse;
        private readonly IMapper _mapper;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fieldResponse = new BookingField();
        }

        /// <summary>
        /// Get all bookings with pagination, sorting, and dynamic field selection
        /// </summary>
        public async Task<PagedResponse<object>> GetAllAsync(BookingQuery queryParams)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsWithDetailsAsync(
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.SearchField,
                    queryParams.Search,
                    queryParams.SortBy ?? queryParams.DefaultSortBy,
                    queryParams.IsDescending);

                var totalItems = await _unitOfWork.Bookings.CountBookingsWithSearchAsync(
                    queryParams.SearchField,
                    queryParams.Search);

                var bookingInfos = bookings.Select(b => new BookingResponse.BookingInfo
                {
                    BookingId = b.BookingId,
                    RenterId = b.RenterId,
                    RenterName = b.Renter?.FullName ?? b.Renter?.Username ?? "Unknown",
                    RenterEmail = b.Renter?.Email ?? "",
                    RenterPhone = b.Renter?.PhoneNumber,
                    PostId = b.PostId,
                    PostTitle = b.Post?.Title ?? "",
                    MeetingTime = b.MeetingTime?.ToVietnamTime(),
                    PlaceMeet = b.PlaceMeet,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt?.ToVietnamTime()
                });

                // Apply field selection if specified
                var selectedFields = queryParams.GetSelectFields();
                if (selectedFields.Any())
                {
                    var filteredData = bookingInfos.Select(item => _fieldResponse.SelectFields(item, selectedFields)).ToList();
                    return new PagedResponse<object>
                    {
                        Items = filteredData,
                        TotalItems = totalItems,
                        CurrentPage = queryParams.Page,
                        TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                    };
                }

                return new PagedResponse<object>
                {
                    Items = bookingInfos.Cast<object>().ToList(),
                    TotalItems = totalItems,
                    CurrentPage = queryParams.Page,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all bookings");
                throw;
            }
        }

        /// <summary>
        /// Get bookings for a specific renter with dynamic field selection
        /// </summary>
        public async Task<PagedResponse<object>> GetBookingsForRenterAsync(int renterId, BookingQuery queryParams)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByRenterIdAsync(
                    renterId,
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.Status,
                    queryParams.SortBy ?? queryParams.DefaultSortBy,
                    queryParams.IsDescending);

                var totalItems = await _unitOfWork.Bookings.CountBookingsByRenterIdAsync(renterId, queryParams.Status);

                var bookingForRenters = bookings.Select(b => new BookingResponse.BookingForRenter
                {
                    BookingId = b.BookingId,
                    PostId = b.PostId,
                    PostTitle = b.Post?.Title ?? "",
                    PostDescription = b.Post?.Description ?? "",
                    PostPrice = b.Post?.Price,
                    PostType = b.Post?.PostType ?? "",
                    LandlordId = b.Post?.UserId ?? 0,
                    LandlordName = b.Post?.User?.FullName ?? b.Post?.User?.Username ?? "Unknown",
                    LandlordEmail = b.Post?.User?.Email ?? "",
                    LandlordPhone = b.Post?.User?.PhoneNumber,
                    Apartment = b.Post?.Apartment != null ? new BookingResponse.ApartmentInfo
                    {
                        ApartmentId = b.Post.Apartment.ApartmentId,
                        ApartmentCode = b.Post.Apartment.ApartmentCode ?? "",
                        Floor = b.Post.Apartment.Floor ?? 0,
                        Area = (double)(b.Post.Apartment.Area ?? 0),
                        NumberBathroom = b.Post.Apartment.NumberBathroom ?? 0,
                        Building = new BookingResponse.BuildingInfo
                        {
                            BuildingId = b.Post.Apartment.Building?.BuildingId ?? 0,
                            BuildingName = b.Post.Apartment.Building?.Name ?? "",
                            BlockCode = b.Post.Apartment.Building?.BlockCode ?? "",
                            Subdivision = new BookingResponse.SubdivisionInfo
                            {
                                SubdivisionId = b.Post.Apartment.Building?.Subdivision?.SubdivisionId.ToString() ?? "",
                                SubdivisionName = b.Post.Apartment.Building?.Subdivision?.Name ?? "",
                                Type = b.Post.Apartment.Building?.Subdivision?.Type,
                                Description = b.Post.Apartment.Building?.Subdivision?.Description
                            }
                        }
                    } : null,
                    MeetingTime = b.MeetingTime?.ToVietnamTime(),
                    PlaceMeet = b.PlaceMeet,
                    MeetingLatitude = b.MeetingLatitude,
                    MeetingLongitude = b.MeetingLongitude,
                    MeetingAddress = b.MeetingAddress,
                    DistanceToMeeting = b.DistanceToMeeting,
                    EstimatedArrivalTime = b.EstimatedArrivalTime?.ToVietnamTime(),
                    IsLocationTrackingEnabled = b.IsLocationTrackingEnabled ?? false,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt?.ToVietnamTime()
                });

                // Apply field selection if specified
                var selectedFields = queryParams.GetSelectFields();
                if (selectedFields.Any())
                {
                    var filteredData = bookingForRenters.Select(item => _fieldResponse.SelectFields(item, selectedFields)).ToList();
                    return new PagedResponse<object>
                    {
                        Items = filteredData,
                        TotalItems = totalItems,
                        CurrentPage = queryParams.Page,
                        TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                    };
                }

                return new PagedResponse<object>
                {
                    Items = bookingForRenters.Cast<object>().ToList(),
                    TotalItems = totalItems,
                    CurrentPage = queryParams.Page,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings for renter {RenterId}", renterId);
                throw;
            }
        }

        /// <summary>
        /// Get bookings for a specific landlord with dynamic field selection
        /// </summary>
        public async Task<PagedResponse<object>> GetBookingsForLandlordAsync(int landlordId, BookingQuery queryParams)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetBookingsByLandlordAsync(
                    landlordId,
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.Status,
                    queryParams.SortBy ?? queryParams.DefaultSortBy,
                    queryParams.IsDescending);

                var totalItems = await _unitOfWork.Bookings.CountBookingsByLandlordAsync(landlordId, queryParams.Status);

                var bookingForLandlords = bookings.Select(b => new BookingResponse.BookingForLandlord
                {
                    BookingId = b.BookingId,
                    RenterId = b.RenterId,
                    RenterName = b.Renter?.FullName ?? b.Renter?.Username ?? "Unknown",
                    RenterEmail = b.Renter?.Email ?? "",
                    RenterPhone = b.Renter?.PhoneNumber,
                    PostId = b.PostId,
                    PostTitle = b.Post?.Title ?? "",
                    Apartment = b.Post?.Apartment != null ? new BookingResponse.ApartmentInfo
                    {
                        ApartmentId = b.Post.Apartment.ApartmentId,
                        ApartmentCode = b.Post.Apartment.ApartmentCode ?? "",
                        Floor = b.Post.Apartment.Floor ?? 0,
                        Area = (double)(b.Post.Apartment.Area ?? 0),
                        NumberBathroom = b.Post.Apartment.NumberBathroom ?? 0,
                        Building = new BookingResponse.BuildingInfo
                        {
                            BuildingId = b.Post.Apartment.Building?.BuildingId ?? 0,
                            BuildingName = b.Post.Apartment.Building?.Name ?? "",
                            BlockCode = b.Post.Apartment.Building?.BlockCode ?? "",
                            Subdivision = new BookingResponse.SubdivisionInfo
                            {
                                SubdivisionId = b.Post.Apartment.Building?.Subdivision?.SubdivisionId.ToString() ?? "",
                                SubdivisionName = b.Post.Apartment.Building?.Subdivision?.Name ?? "",
                                Type = b.Post.Apartment.Building?.Subdivision?.Type,
                                Description = b.Post.Apartment.Building?.Subdivision?.Description
                            }
                        }
                    } : null,
                    MeetingTime = b.MeetingTime?.ToVietnamTime(),
                    PlaceMeet = b.PlaceMeet,
                    MeetingLatitude = b.MeetingLatitude,
                    MeetingLongitude = b.MeetingLongitude,
                    MeetingAddress = b.MeetingAddress,
                    DistanceToMeeting = b.DistanceToMeeting,
                    EstimatedArrivalTime = b.EstimatedArrivalTime?.ToVietnamTime(),
                    IsLocationTrackingEnabled = b.IsLocationTrackingEnabled ?? false,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt?.ToVietnamTime()
                });

                // Apply field selection if specified
                var selectedFields = queryParams.GetSelectFields();
                if (selectedFields.Any())
                {
                    var filteredData = bookingForLandlords.Select(item => _fieldResponse.SelectFields(item, selectedFields)).ToList();
                    return new PagedResponse<object>
                    {
                        Items = filteredData,
                        TotalItems = totalItems,
                        CurrentPage = queryParams.Page,
                        TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                    };
                }

                return new PagedResponse<object>
                {
                    Items = bookingForLandlords.Cast<object>().ToList(),
                    TotalItems = totalItems,
                    CurrentPage = queryParams.Page,
                    TotalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings for landlord {LandlordId}", landlordId);
                throw;
            }
        }

        /// <summary>
        /// Get booking by ID with dynamic field selection
        /// </summary>
        public async Task<object?> GetByIdAsync(int bookingId, List<string>? selectedFields = null)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
                
                if (booking == null)
                {
                    return null;
                }

                var bookingDetail = new BookingResponse.BookingDetail
                {
                    BookingId = booking.BookingId,
                    RenterId = booking.RenterId,
                    RenterName = booking.Renter?.FullName ?? booking.Renter?.Username ?? "Unknown",
                    RenterEmail = booking.Renter?.Email ?? "",
                    RenterPhone = booking.Renter?.PhoneNumber,
                    PostId = booking.PostId,
                    PostTitle = booking.Post?.Title ?? "",
                    PostDescription = booking.Post?.Description ?? "",
                    PostPrice = booking.Post?.Price,
                    LandlordId = booking.Post?.UserId ?? 0,
                    LandlordName = booking.Post?.User?.FullName ?? booking.Post?.User?.Username ?? "Unknown",
                    LandlordEmail = booking.Post?.User?.Email ?? "",
                    LandlordPhone = booking.Post?.User?.PhoneNumber,
                    MeetingTime = booking.MeetingTime?.ToVietnamTime(),
                    PlaceMeet = booking.PlaceMeet,
                    Status = booking.Status,
                    CreatedAt = booking.CreatedAt?.ToVietnamTime()
                };

                // Apply field selection if specified
                if (selectedFields?.Any() == true)
                {
                    return _fieldResponse.SelectFields(bookingDetail, selectedFields);
                }

                return bookingDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Get booking detail by ID (full details without field selection)
        /// </summary>
        public async Task<BookingResponse.BookingDetail?> GetDetailAsync(int bookingId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
                
                if (booking == null)
                {
                    return null;
                }

                return new BookingResponse.BookingDetail
                {
                    BookingId = booking.BookingId,
                    RenterId = booking.RenterId,
                    RenterName = booking.Renter?.FullName ?? booking.Renter?.Username ?? "Unknown",
                    RenterEmail = booking.Renter?.Email ?? "",
                    RenterPhone = booking.Renter?.PhoneNumber,
                    PostId = booking.PostId,
                    PostTitle = booking.Post?.Title ?? "",
                    PostDescription = booking.Post?.Description ?? "",
                    PostPrice = booking.Post?.Price,
                    LandlordId = booking.Post?.UserId ?? 0,
                    LandlordName = booking.Post?.User?.FullName ?? booking.Post?.User?.Username ?? "Unknown",
                    LandlordEmail = booking.Post?.User?.Email ?? "",
                    LandlordPhone = booking.Post?.User?.PhoneNumber,
                    MeetingTime = booking.MeetingTime?.ToVietnamTime(),
                    PlaceMeet = booking.PlaceMeet,
                    Status = booking.Status,
                    CreatedAt = booking.CreatedAt?.ToVietnamTime()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking detail {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        public async Task<int> CreateBookingAsync(BookingRequest.BookingCreate request, int renterId)
        {
            try
            {
                // Check if post exists
                var post = await _unitOfWork.Posts.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    throw new ArgumentException("Post not found");
                }

                // Check if post owner is trying to book their own post
                if (post.UserId == renterId)
                {
                    throw new ArgumentException("Cannot book your own post");
                }

                // Check if renter can create booking (no existing pending/confirmed booking)
                var canCreate = await _unitOfWork.Bookings.CanCreateBookingAsync(renterId, request.PostId);
                if (!canCreate)
                {
                    throw new ArgumentException("You already have a pending or confirmed booking for this post");
                }

                var booking = new Booking
                {
                    RenterId = renterId,
                    PostId = request.PostId,
                    MeetingTime = request.MeetingTime,
                    PlaceMeet = request.PlaceMeet,
                    Status = "Pending",
                    CreatedAt = TimeZoneConverter.GetVietnamCurrentTime()
                };

                _unitOfWork.Bookings.PrepareCreate(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Booking created successfully with ID {BookingId}", booking.BookingId);
                return booking.BookingId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking for renter {RenterId}", renterId);
                throw;
            }
        }

        /// <summary>
        /// Update booking details (only by renter or landlord)
        /// </summary>
        public async Task<bool> UpdateBookingAsync(int bookingId, BookingRequest.BookingUpdate request, int userId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    return false;
                }

                // Check authorization - only renter or landlord can update
                if (booking.RenterId != userId && booking.Post?.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this booking");
                }

                // Update fields if provided
                if (request.MeetingTime.HasValue)
                    booking.MeetingTime = request.MeetingTime.Value;

                if (!string.IsNullOrWhiteSpace(request.PlaceMeet))
                    booking.PlaceMeet = request.PlaceMeet;

                if (!string.IsNullOrWhiteSpace(request.Status))
                    booking.Status = request.Status;

                _unitOfWork.Bookings.PrepareUpdate(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingId} updated successfully by user {UserId}", bookingId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Update booking status (mainly for landlord to accept/reject)
        /// </summary>
        public async Task<bool> UpdateBookingStatusAsync(int bookingId, BookingRequest.BookingStatusUpdate request, int userId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    return false;
                }

                // Check authorization - only renter or landlord can update status
                if (booking.RenterId != userId && booking.Post?.UserId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update this booking status");
                }

                booking.Status = request.Status;
                
                _unitOfWork.Bookings.PrepareUpdate(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingId} status updated to {Status} by user {UserId}", bookingId, request.Status, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking status {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Delete booking (only by renter who created it)
        /// </summary>
        public async Task<bool> DeleteBookingAsync(int bookingId, int userId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    return false;
                }

                // Check authorization - only renter can delete their booking
                if (booking.RenterId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to delete this booking");
                }

                _unitOfWork.Bookings.PrepareRemove(booking);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Booking {BookingId} deleted successfully by user {UserId}", bookingId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Check if renter can create booking for a specific post
        /// </summary>
        public async Task<bool> CanCreateBookingAsync(int renterId, int postId)
        {
            try
            {
                return await _unitOfWork.Bookings.CanCreateBookingAsync(renterId, postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if renter {RenterId} can create booking for post {PostId}", renterId, postId);
                throw;
            }
        }
    }
}