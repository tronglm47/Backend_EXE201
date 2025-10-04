using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Repositories.Constants;
using Services.RequestsResponses;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.Apartment;
using Services.RequestsResponses.Building;
using Services.RequestsResponses.Subdivision;

namespace Services
{
    public interface IPostService
    {
        Task<PagedResponse<object>> GetAllAsync(PostQuery queryParams);
        Task<PagedResponse<object>> GetAllPostsForLandLordAsync(PostQuery queryParams);
        Task<PagedResponse<object>> GetAllPostsForUserAsync(PostQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<PostResponse.PostDetailForLandLord?> GetDetailForLandLordAsync(int id);
        Task<PostResponse.PostDetailForUser?> GetDetailForUserAsync(int id);
        Task<int> CreatePostForUserAsync(PostRequest.PostCreateForUser request, int userId);
        Task<int> CreatePostForLandLordAsync(PostRequest.PostCreateForLandLord request, int userId);
        Task<bool> UpdatePostForUserAsync(PostRequest.PostUpdateForUser request, int postId, int userId);
        Task<bool> UpdatePostForLandLordAsync(PostRequest.PostUpdateForLandLord request, int postId, int userId);
        Task<bool> DeletePostAsync(int postId, int userId);
    }
    
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostService> _logger;
        private readonly PostField _fieldResponse;
        private readonly IMapper _mapper;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PostService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new PostField();
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all posts with pagination, sorting, and dynamic search
        /// </summary>
        public async Task<PagedResponse<object>> GetAllAsync(PostQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.Posts.CountPostsWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get posts with advanced query (dynamic search)
            var posts = await _unitOfWork.Posts.GetPostsWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,  // Dynamic search field
                search: queryParams.Search,            // Dynamic search value
                sortBy: queryParams.SortBy ?? "PostId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = posts.Select(p =>
                {
                    var postResponse = _mapper.Map<PostResponse.PostGetAll>(p);
                    return _fieldResponse.SelectFields(postResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        /// <summary>
        /// Get post by ID with optional field selection
        /// </summary>
        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null || post.PostId == 0)
            {
                return null;
            }

            var postResponse = _mapper.Map<PostResponse.PostGetAll>(post);
            return _fieldResponse.SelectFields(postResponse, selectedFields);
        }

        /// <summary>
        /// Get all posts for User with full details (Apartment, Building, Subdivision)
        /// Includes navigation properties for detailed information display
        /// Only returns posts with PostType = ForRent or ForSale
        /// </summary>


        /// <summary>
        /// Get all posts for landlord with full details (Apartment, Building, Subdivision)
        /// Includes navigation properties for detailed information display
        /// Only returns posts with PostType = ForRent or ForSale
        /// </summary>
        public async Task<PagedResponse<object>> GetAllPostsForLandLordAsync(PostQuery queryParams)
        {
            // Count total items with search (only ForRent and ForSale)
            var totalItems = await _unitOfWork.Posts.CountPostsForLandLordAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get posts with full details (including navigation properties)
            var posts = await _unitOfWork.Posts.GetPostsForLandLordWithDetailsAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,
                search: queryParams.Search,
                sortBy: queryParams.SortBy ?? "PostId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = posts
                    .Where(p => p.Apartment != null) // Only include posts with apartments
                    .Select(p =>
                    {
                        var landlordResponse = new PostResponse.PostGetAllForLandLord
                        {
                            PostId = p.PostId,
                            UserId = p.UserId,
                            UserName = p.User?.Username ?? "Unknown",
                            Title = p.Title,
                            Description = p.Description,
                            Price = (double?)p.Price,
                            PostType = p.PostType,
                            Status = p.Status,
                            CreatedAt = p.CreatedAt ?? DateTime.UtcNow,
                            // Apartment details
                            ApartmentId = p.ApartmentId,
                            ApartmentCode = p.Apartment?.ApartmentCode ?? "",
                            Floor = p.Apartment?.Floor ?? 0,
                            Area = (double)(p.Apartment?.Area ?? 0),
                            NumberBathroom = p.Apartment?.NumberBathroom ?? 0,
                            // Building details
                            BuildingId = p.Apartment?.BuildingId ?? 0,
                            BuildingName = p.Apartment?.Building?.Name ?? "",
                            BlockCode = p.Apartment?.Building?.BlockCode ?? "",
                            // Subdivision details
                            SubdivisionId = p.Apartment?.Building?.SubdivisionId.ToString() ?? "",
                            SubdivisionName = p.Apartment?.Building?.Subdivision?.Name ?? ""
                        };

                        // If select fields specified, filter the response
                        if (selectedFields.Any())
                        {
                            return _fieldResponse.SelectFields(landlordResponse, selectedFields);
                        }
                        return landlordResponse;
                    })
                    .ToList()
            };

            return response;
        }

        /// <summary>
        /// Get detailed information for a single post for landlord (ForRent or ForSale)
        /// Returns full nested details including Apartment, Building, and Subdivision information
        /// Returns null if post not found or not a landlord post type
        /// </summary>
        public async Task<PostResponse.PostDetailForLandLord?> GetDetailForLandLordAsync(int id)
        {
            // Get post with full details (including navigation properties)
            var post = await _unitOfWork.Posts.GetByIdForLandLordAsync(id);

            if (post == null)
            {
                _logger.LogWarning("GetDetailForLandLordAsync: Post with ID {PostId} not found or not a landlord post", id);
                return null;
            }

            if (post.Apartment == null)
            {
                _logger.LogWarning("GetDetailForLandLordAsync: Post {PostId} has no apartment information", id);
                return null;
            }

            // Manual mapping to PostDetailForLandLord with full nested structure
            var response = new PostResponse.PostDetailForLandLord
            {
                PostId = post.PostId,
                ApartmentId = post.ApartmentId,
                UserId = post.UserId,
                UserName = post.User?.Username ?? "Unknown",
                Title = post.Title,
                Description = post.Description,
                Price = (double?)post.Price,
                PostType = post.PostType,
                Status = post.Status,
                CreatedAt = post.CreatedAt ?? DateTime.UtcNow,
                Apartment = new ApartmentResponse.ApartmentDetail
                {
                    ApartmentId = post.Apartment.ApartmentId,
                    BuildingId = post.Apartment.BuildingId ?? 0,
                    ApartmentCode = post.Apartment.ApartmentCode,
                    Floor = post.Apartment.Floor ?? 0,
                    Area = (double)(post.Apartment.Area ?? 0),
                    ApartmentType = post.Apartment.ApartmentType ?? "",
                    Status = post.Apartment.Status,
                    NumberOfBedrooms = post.Apartment.NumberBathroom ?? 0, // Note: Model has NumberBathroom
                    CreatedAt = post.Apartment.CreatedAt ?? DateTime.UtcNow,
                    Building = post.Apartment.Building != null ? new BuildingResponse.BuildingDetail
                    {
                        BuildingId = post.Apartment.Building.BuildingId,
                        SubdivisionId = post.Apartment.Building.SubdivisionId,
                        Name = post.Apartment.Building.Name,
                        BlockCode = post.Apartment.Building.BlockCode,
                        Description = "", // Building model doesn't have Description field
                        CreatedAt = post.Apartment.Building.CreatedAt,
                        Subdivision = post.Apartment.Building.Subdivision != null ? new SubdivisionResponse.SubdivisionDetail
                        {
                            SubdivisionId = post.Apartment.Building.Subdivision.SubdivisionId,
                            Name = post.Apartment.Building.Subdivision.Name,
                            Type = post.Apartment.Building.Subdivision.Type,
                            Description = post.Apartment.Building.Subdivision.Description,
                            CreatedAt = post.Apartment.Building.Subdivision.CreatedAt
                        } : null!
                    } : null!
                }
            };

            _logger.LogInformation("Successfully retrieved detail for landlord post {PostId}", id);
            return response;
        }

        /// <summary>
        /// Get all posts for user with user details
        /// Only returns posts with PostType = FindRoom
        /// Shows title, description, and user information
        /// </summary>
        public async Task<PagedResponse<object>> GetAllPostsForUserAsync(PostQuery queryParams)
        {
            // Count total items with search (only FindRoom)
            var totalItems = await _unitOfWork.Posts.CountPostsForUserAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get posts with user details
            var posts = await _unitOfWork.Posts.GetPostsForUserAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,
                search: queryParams.Search,
                sortBy: queryParams.SortBy ?? "PostId",
                isDescending: queryParams.IsDescending
            );

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = posts.Select(post => new PostResponse.PostGetAllForUser
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = post.User?.Username ?? "Unknown",
                    Title = post.Title,
                    Description = post.Description
                }).ToList<object>()
            };

            _logger.LogInformation("Successfully retrieved {Count} user posts (page {Page})", posts.Count(), queryParams.Page);
            return response;
        }

        /// <summary>
        /// Get detailed information for a single post for user (FindRoom)
        /// Returns post details with user information
        /// Returns null if post not found or not a user post type
        /// </summary>
        public async Task<PostResponse.PostDetailForUser?> GetDetailForUserAsync(int id)
        {
            _logger.LogInformation("Getting detail for user post {PostId}", id);

            var post = await _unitOfWork.Posts.GetByIdForUserAsync(id);

            if (post == null)
            {
                _logger.LogWarning("User post {PostId} not found or not FindRoom type", id);
                return null;
            }

            var response = new PostResponse.PostDetailForUser
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = post.User?.Username ?? "Unknown",
                Title = post.Title,
                Description = post.Description,
                CreatedAt = post.CreatedAt ?? DateTime.UtcNow
            };

            _logger.LogInformation("Successfully retrieved detail for user post {PostId}", id);
            return response;
        }

        /// <summary>
        /// Create a post for regular user (simple post - finding room)
        /// PostType will be set to "FindRoom"
        /// Status will be set to "Active"
        /// No apartment information needed
        /// </summary>
        public async Task<int> CreatePostForUserAsync(PostRequest.PostCreateForUser request, int userId)
        {
            if (request == null)
            {
                _logger.LogWarning("CreatePostForUserAsync: Request is null");
                return 0;
            }

            try
            {
                // Create post for user (finding room)
                var post = new Post
                {
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    PostType = PostTypeConstants.FindRoom, // User is finding room
                    Status = PostStatusConstants.Active,
                    CreatedAt = DateTime.UtcNow,
                    ApartmentId = null, // No apartment for user post
                    Price = null // No price for finding room post
                };

                var result = await _unitOfWork.Posts.CreateAsync(post);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Post created successfully for User {UserId}, PostId: {PostId}", userId, post.PostId);
                return result > 0 ? post.PostId : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Create a post for landlord (rental post with apartment details)
        /// PostType will be set to "ForRent"
        /// Status can be customized by landlord
        /// Apartment information is required and will be created
        /// </summary>
        public async Task<int> CreatePostForLandLordAsync(PostRequest.PostCreateForLandLord request, int userId)
        {
            if (request == null)
            {
                _logger.LogWarning("CreatePostForLandLordAsync: Request is null");
                return 0;
            }

            if (request.Apartment == null)
            {
                _logger.LogWarning("CreatePostForLandLordAsync: Apartment information is required");
                return 0;
            }

            try
            {
                // Verify building exists
                var building = await _unitOfWork.Buildings.GetByIdAsync(request.Apartment.BuildingId);
                if (building == null)
                {
                    _logger.LogWarning("Building with ID {BuildingId} not found", request.Apartment.BuildingId);
                    return 0;
                }

                // Create apartment first
                var apartment = new Apartment
                {
                    BuildingId = request.Apartment.BuildingId,
                    ApartmentCode = request.Apartment.ApartmentCode,
                    Floor = request.Apartment.Floor,
                    Area = (decimal)request.Apartment.Area,
                    ApartmentType = request.Apartment.ApartmentType,
                    Status = request.Apartment.Status ?? "Available",
                    NumberBathroom = request.Apartment.NumberOfBedrooms,
                    CreatedAt = DateTime.UtcNow
                };

                var apartmentResult = await _unitOfWork.Apartments.CreateAsync(apartment);
                await _unitOfWork.SaveChangesAsync();

                if (apartmentResult == 0)
                {
                    _logger.LogError("Failed to create apartment for landlord post");
                    return 0;
                }

                // Create post for landlord (rental listing)
                var post = new Post
                {
                    UserId = userId,
                    ApartmentId = apartment.ApartmentId,
                    Title = request.Title,
                    Description = request.Description,
                    Price = (decimal)request.Price,
                    PostType = PostTypeConstants.ForRent, // Landlord is offering room for rent
                    Status = request.status ?? PostStatusConstants.Active,
                    CreatedAt = DateTime.UtcNow
                };

                var postResult = await _unitOfWork.Posts.CreateAsync(post);
                await _unitOfWork.SaveChangesAsync();

                // Handle utilities if provided
                if (request.UtilityIds != null && request.UtilityIds.Any())
                {
                    // Validate that all utility IDs exist
                    var existingUtilityIds = await _unitOfWork.Utilities.GetExistingUtilityIdsAsync(request.UtilityIds);
                    
                    var invalidUtilityIds = request.UtilityIds.Except(existingUtilityIds).ToList();
                    if (invalidUtilityIds.Any())
                    {
                        _logger.LogWarning("Invalid utility IDs found: {InvalidIds}", string.Join(", ", invalidUtilityIds));
                        throw new ArgumentException($"Invalid utility IDs: {string.Join(", ", invalidUtilityIds)}");
                    }

                    // Create PostUtility records
                    foreach (var utilityId in request.UtilityIds)
                    {
                        var postUtility = new PostUtility
                        {
                            PostId = post.PostId,
                            UtilityId = utilityId,
                            Notes = null
                        };
                        _unitOfWork.PostUtilities.PrepareCreate(postUtility);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Created {Count} utility associations for Post {PostId}", request.UtilityIds.Count, post.PostId);
                }

                _logger.LogInformation("Post created successfully for LandLord {UserId}, PostId: {PostId}, ApartmentId: {ApartmentId}", 
                    userId, post.PostId, apartment.ApartmentId);
                
                return postResult > 0 ? post.PostId : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for landlord {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Update a post for regular user (finding room)
        /// Only title and description can be updated
        /// PostType must be "FindRoom"
        /// </summary>
        public async Task<bool> UpdatePostForUserAsync(PostRequest.PostUpdateForUser request, int postId, int userId)
        {
            if (request == null)
            {
                _logger.LogWarning("UpdatePostForUserAsync: Request is null");
                return false;
            }

            try
            {
                // Get existing post
                var post = await _unitOfWork.Posts.GetByIdAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post {PostId} not found", postId);
                    return false;
                }

                // Verify ownership
                if (post.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not the owner of post {PostId}", userId, postId);
                    return false;
                }

                // Verify post type
                if (post.PostType != PostTypeConstants.FindRoom)
                {
                    _logger.LogWarning("Post {PostId} is not a FindRoom post", postId);
                    return false;
                }

                // Update post
                post.Title = request.Title;
                post.Description = request.Description;

                var result = await _unitOfWork.Posts.UpdateAsync(post);
                
                _logger.LogInformation("Post {PostId} updated successfully by User {UserId}", postId, userId);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId} for user {UserId}", postId, userId);
                throw;
            }
        }

        /// <summary>
        /// Update a post for landlord (rental listing)
        /// Can update post details and apartment information
        /// PostType must be "ForRent"
        /// </summary>
        public async Task<bool> UpdatePostForLandLordAsync(PostRequest.PostUpdateForLandLord request, int postId, int userId)
        {
            if (request == null)
            {
                _logger.LogWarning("UpdatePostForLandLordAsync: Request is null");
                return false;
            }

            if (request.Apartment == null)
            {
                _logger.LogWarning("UpdatePostForLandLordAsync: Apartment information is required");
                return false;
            }

            try
            {
                // Get existing post
                var post = await _unitOfWork.Posts.GetByIdAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post {PostId} not found", postId);
                    return false;
                }

                // Verify ownership
                if (post.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not the owner of post {PostId}", userId, postId);
                    return false;
                }

                // Verify post type
                if (post.PostType != PostTypeConstants.ForRent)
                {
                    _logger.LogWarning("Post {PostId} is not a ForRent post", postId);
                    return false;
                }

                // Verify apartment exists
                if (post.ApartmentId == null)
                {
                    _logger.LogWarning("Post {PostId} has no associated apartment", postId);
                    return false;
                }

                var apartment = await _unitOfWork.Apartments.GetByIdAsync(post.ApartmentId.Value);
                if (apartment == null)
                {
                    _logger.LogWarning("Apartment {ApartmentId} not found", post.ApartmentId.Value);
                    return false;
                }

                // Update apartment information
                apartment.ApartmentCode = request.Apartment.ApartmentCode;
                apartment.Floor = request.Apartment.Floor;
                apartment.Area = (decimal)request.Apartment.Area;
                apartment.ApartmentType = request.Apartment.ApartmentType;
                apartment.Status = request.Apartment.Status ?? apartment.Status;

                await _unitOfWork.Apartments.UpdateAsync(apartment);

                // Update post
                post.Title = request.Title;
                post.Description = request.Description;
                post.Price = (decimal)request.Price;
                post.Status = request.status ?? post.Status;

                var result = await _unitOfWork.Posts.UpdateAsync(post);

                // Handle utilities update (Option A: Delete all old utilities and create new ones)
                if (request.UtilityIds != null)
                {
                    // Step 1: Delete all existing utilities for this post
                    var existingPostUtilities = await _unitOfWork.PostUtilities
                        .GetAllAsync();
                    var postUtilitiesToRemove = existingPostUtilities
                        .Where(pu => pu.PostId == postId)
                        .ToList();

                    foreach (var postUtility in postUtilitiesToRemove)
                    {
                        _unitOfWork.PostUtilities.PrepareRemove(postUtility);
                    }
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} existing utilities for Post {PostId}", postUtilitiesToRemove.Count, postId);

                    // Step 2: Validate new utility IDs
                    if (request.UtilityIds.Any())
                    {
                        var existingUtilityIds = await _unitOfWork.Utilities.GetExistingUtilityIdsAsync(request.UtilityIds);
                        
                        var invalidUtilityIds = request.UtilityIds.Except(existingUtilityIds).ToList();
                        if (invalidUtilityIds.Any())
                        {
                            _logger.LogWarning("Invalid utility IDs found: {InvalidIds}", string.Join(", ", invalidUtilityIds));
                            throw new ArgumentException($"Invalid utility IDs: {string.Join(", ", invalidUtilityIds)}");
                        }

                        // Step 3: Create new PostUtility records
                        foreach (var utilityId in request.UtilityIds)
                        {
                            var postUtility = new PostUtility
                            {
                                PostId = postId,
                                UtilityId = utilityId,
                                Notes = null
                            };
                            _unitOfWork.PostUtilities.PrepareCreate(postUtility);
                        }
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Created {Count} new utility associations for Post {PostId}", request.UtilityIds.Count, postId);
                    }
                }
                
                _logger.LogInformation("Post {PostId} and Apartment {ApartmentId} updated successfully by LandLord {UserId}", 
                    postId, apartment.ApartmentId, userId);
                
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId} for landlord {UserId}", postId, userId);
                throw;
            }
        }

        /// <summary>
        /// Delete a post (soft delete by setting status to "Deleted")
        /// Only the owner can delete their post
        /// </summary>
        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            try
            {
                // Get existing post
                var post = await _unitOfWork.Posts.GetByIdAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning("Post {PostId} not found", postId);
                    return false;
                }

                // Verify ownership
                if (post.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not the owner of post {PostId}", userId, postId);
                    return false;
                }

                // Soft delete - set status to Deleted
                post.Status = PostStatusConstants.Deleted;
                var result = await _unitOfWork.Posts.UpdateAsync(post);

                _logger.LogInformation("Post {PostId} deleted successfully by User {UserId}", postId, userId);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId} by user {UserId}", postId, userId);
                throw;
            }
        }
    }
}
