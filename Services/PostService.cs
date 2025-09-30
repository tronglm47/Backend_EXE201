using AutoMapper;
using Repositories;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.Post;
using Services.RequestsResponses.PostAmenity;
using Services.RequestsResponses.PostType;
using Services.RequestsResponses.PropertyForm;
using Services.RequestsResponses.PropertyType;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPostService
    {
        Task<PagedResponse<object>> GetAllPostAsync(PostQueryParameters queryParams);
        Task<object?> GetPostById(int id, List<string> selectedFields);
        Task<int> Create(PostRequest.CreatePost item);
        Task<bool> Update(PostRequest.PostUpdateRequest item, int id);
        Task<bool> Delete(int id);
    }
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PostFieldResponse _postFieldResponse;
        private readonly IMapper _mapper;
        private readonly ICloudStorageService _cloudStorageService;

        public PostService(IUnitOfWork unitOfWork, IMapper mapper, ICloudStorageService cloudStorageService)
        {
            _unitOfWork = unitOfWork;
            _postFieldResponse = new PostFieldResponse();
            _mapper = mapper;
            _cloudStorageService = cloudStorageService;
        }

        public async Task<PagedResponse<object>> GetAllPostAsync(PostQueryParameters queryParams)
        {
            var postRepository = _unitOfWork.Posts as PostRepository;
            if (postRepository == null)
                throw new InvalidOperationException("PostRepository not available");

            var totalItems = await postRepository.CountWithSearch(queryParams.Search);
            var posts = await postRepository.GetPostsWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "PostId",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = posts.Select(c =>
                {
                    var postResponse = new PostResponse.PostGetAllResponse
                    {
                        PostId = c.PostId,
                        UserId = c.UserId,
                        PostTypeId = c.PostTypeId,
                        PropertyTypeId = c.PropertyTypeId,
                        PropertyFormId = c.PropertyFormId,
                        LocationId = c.LocationId,
                        Title = c.Title,
                        Content = c.Content,
                        Images = c.Images,
                        Price = c.Price,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt,
                        Views = c.Views
                    };

                    return _postFieldResponse.SelectFields(postResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetPostById(int id, List<string> selectedFields)
        {
            var postRepository = _unitOfWork.Posts as PostRepository;
            if (postRepository == null)
                throw new InvalidOperationException("PostRepository not available");

            var post = await postRepository.GetByIdAdvancedAsync(id);
            if (post == null || post.PostId == 0)
            {
                return null;
            }

            // Extract amenity IDs and amenities from loaded navigation properties
            var amenityIds = post.PostAmenities?.Select(pa => pa.AmenityId).ToList() ?? new List<int>();
            var amenities = post.PostAmenities?.Select(pa => pa.Amenity).Where(a => a != null).ToList() ?? new List<Amenity>();

            var postResponse = new PostResponse.PostGetByIdResponse
            {
                PostId = post.PostId,
                UserId = post.UserId,
                PostTypeId = post.PostTypeId,
                PostType = post.PostType != null ? _mapper.Map<PostTypeResponse.PostTypeGetById>(post.PostType) : null!,
                PropertyTypeId = post.PropertyTypeId,
                PropertyType = post.PropertyType != null ? _mapper.Map<PropertyTypeResponse.PropertyTypeGetByIdResponse>(post.PropertyType) : null!,
                PropertyFormId = post.PropertyFormId,
                PropertyForm = post.PropertyForm != null ? _mapper.Map<PropertyFormResponse.PropertyFormGetByIdResponse>(post.PropertyForm) : null!,
                LocationId = post.LocationId,
                AmenitiesId = amenityIds,
                Amenities = amenities.Select(a => _mapper.Map<AmenityResponse.GetByIdResponse>(a)).ToList(),
                Title = post.Title,
                Content = post.Content,
                Images = post.Images,
                Price = post.Price,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                Views = post.Views
            };
            return _postFieldResponse.SelectFields(postResponse, selectedFields);
        }
        public async Task<int> Create(PostRequest.CreatePost item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Post data cannot be null");
            }

            // Validate AmenityIds để đảm bảo không có duplicate
            if (item.AmenityId.Count != item.AmenityId.Distinct().Count())
            {
                throw new ArgumentException("Duplicate amenity IDs are not allowed");
            }

            // Validate tất cả AmenityIds có tồn tại không
            foreach (var amenityId in item.AmenityId)
            {
                if (amenityId <= 0)
                {
                    throw new ArgumentException($"Invalid amenity ID: {amenityId}. All amenity IDs must be greater than 0");
                }

                var amenity = await _unitOfWork.Amenities.GetByIdAsync(amenityId);
                if (amenity == null)
                {
                    throw new ArgumentException($"Amenity with ID {amenityId} does not exist");
                }
            }

            // Validate PropertyType exists
            var propertyType = await _unitOfWork.PropertyTypes.GetByIdAsync(item.PropertyTypeId);
            if (propertyType == null)
            {
                throw new ArgumentException($"PropertyType with ID {item.PropertyTypeId} does not exist");
            }

            // Validate PropertyForm exists
            var propertyForm = await _unitOfWork.PropertyForms.GetByIdAsync(item.PropertyFormId);
            if (propertyForm == null)
            {
                throw new ArgumentException($"PropertyForm with ID {item.PropertyFormId} does not exist");
            }

            // Tạo Post từ request (không bao gồm AmenityId)
            var post = _mapper.Map<Post>(item);
            
            // Tạo Post trong database
            var result = await _unitOfWork.Posts.CreateAsync(post);
            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to create post in database");
            }

            // Lưu để lấy PostId
            await _unitOfWork.SaveChangesAsync();

            // Tạo các mối quan hệ PostAmenity
            if (item.AmenityId != null && item.AmenityId.Any())
            {
                var postAmenities = item.AmenityId.Select(amenityId => new PostAmenity
                {
                    PostId = post.PostId,
                    AmenityId = amenityId,
                    Notes = null // Có thể thêm notes nếu cần
                }).ToList();

                // Thêm từng PostAmenity
                foreach (var postAmenity in postAmenities)
                {
                    await _unitOfWork.PostAmenity.CreateAsync(postAmenity);
                }

                // Lưu tất cả PostAmenity
                await _unitOfWork.SaveChangesAsync();
            }

            return post.PostId;
        }

        public async Task<bool> Update(PostRequest.PostUpdateRequest item, int id)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Post update data cannot be null");
            }

            // Get existing post
            var existingPost = await _unitOfWork.Posts.GetByIdAsync(id);
            if (existingPost == null)
            {
                return false;
            }

            // Validate PropertyType exists if provided
            if (item.PropertyTypeId > 0)
            {
                var propertyType = await _unitOfWork.PropertyTypes.GetByIdAsync(item.PropertyTypeId);
                if (propertyType == null)
                {
                    throw new ArgumentException($"PropertyType with ID {item.PropertyTypeId} does not exist");
                }
            }

            // Validate PropertyForm exists if provided
            if (item.PropertyFormId > 0)
            {
                var propertyForm = await _unitOfWork.PropertyForms.GetByIdAsync(item.PropertyFormId);
                if (propertyForm == null)
                {
                    throw new ArgumentException($"PropertyForm with ID {item.PropertyFormId} does not exist");
                }
            }

            // Validate AmenityIds if provided
            if (item.AmenityId != null && item.AmenityId.Any())
            {
                // Validate AmenityIds để đảm bảo không có duplicate
                if (item.AmenityId.Count != item.AmenityId.Distinct().Count())
                {
                    throw new ArgumentException("Duplicate amenity IDs are not allowed");
                }

                // Validate tất cả AmenityIds có tồn tại không
                foreach (var amenityId in item.AmenityId)
                {
                    if (amenityId <= 0)
                    {
                        throw new ArgumentException($"Invalid amenity ID: {amenityId}. All amenity IDs must be greater than 0");
                    }

                    var amenity = await _unitOfWork.Amenities.GetByIdAsync(amenityId);
                    if (amenity == null)
                    {
                        throw new ArgumentException($"Amenity with ID {amenityId} does not exist");
                    }
                }
            }

            // Update post properties
            existingPost.UserId = item.UserId;
            existingPost.PostTypeId = item.PostTypeId;
            existingPost.PropertyTypeId = item.PropertyTypeId;
            existingPost.PropertyFormId = item.PropertyFormId;
            if (item.LocationId.HasValue)
            {
                existingPost.LocationId = item.LocationId.Value;
            }
            existingPost.Title = item.Title;
            existingPost.Content = item.Content;
            existingPost.Images = item.Images;
            if (item.Price.HasValue)
            {
                existingPost.Price = item.Price.Value;
            }
            existingPost.Status = item.Status;

            // Update post in database
            var result = await _unitOfWork.Posts.UpdateAsync(existingPost);
            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to update post in database");
            }

            // Handle amenity relationships update if provided
            if (item.AmenityId != null)
            {
                // First, remove all existing PostAmenity records for this post
                var existingPostAmenities = await _unitOfWork.PostAmenity.GetAllAsync();
                var relatedPostAmenities = existingPostAmenities.Where(pa => pa.PostId == id).ToList();

                foreach (var postAmenity in relatedPostAmenities)
                {
                    await _unitOfWork.PostAmenity.RemoveAsync(postAmenity);
                }

                // Then, add new PostAmenity records
                if (item.AmenityId.Any())
                {
                    var newPostAmenities = item.AmenityId.Select(amenityId => new PostAmenity
                    {
                        PostId = id,
                        AmenityId = amenityId,
                        Notes = null
                    }).ToList();

                    foreach (var postAmenity in newPostAmenities)
                    {
                        await _unitOfWork.PostAmenity.CreateAsync(postAmenity);
                    }
                }

                // Save changes for amenity relationships
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid post ID");
            }

            // Get existing post
            var existingPost = await _unitOfWork.Posts.GetByIdAsync(id);
            if (existingPost == null)
            {
                return false;
            }

            // Delete images from Google Cloud Storage before deleting post
            if (!string.IsNullOrWhiteSpace(existingPost.Images))
            {
                try
                {
                    // Parse image URLs (assuming they are stored as comma-separated or JSON array)
                    var imageUrls = ParseImageUrls(existingPost.Images);
                    if (imageUrls.Any())
                    {
                        await _cloudStorageService.DeleteImagesAsync(imageUrls);
                    }
                }
                catch (Exception)
                {
                    // Log but don't fail deletion if image cleanup fails
                    // The post data is more important than the images
                }
            }

            // First, delete all related PostAmenity records
            var postAmenities = await _unitOfWork.PostAmenity.GetAllAsync();
            var relatedPostAmenities = postAmenities.Where(pa => pa.PostId == id).ToList();

            foreach (var postAmenity in relatedPostAmenities)
            {
                await _unitOfWork.PostAmenity.RemoveAsync(postAmenity);
            }

            // Then delete the post
            var result = await _unitOfWork.Posts.RemoveAsync(existingPost);
            if (!result)
            {
                throw new InvalidOperationException("Failed to delete post from database");
            }

            return true;
        }

        private List<string> ParseImageUrls(string images)
        {
            if (string.IsNullOrWhiteSpace(images))
            {
                return new List<string>();
            }

            try
            {
                // Try to parse as JSON array first
                if (images.TrimStart().StartsWith("["))
                {
                    var urls = System.Text.Json.JsonSerializer.Deserialize<string[]>(images);
                    return urls?.Where(url => !string.IsNullOrWhiteSpace(url)).ToList() ?? new List<string>();
                }
                
                // If not JSON, treat as comma-separated values
                return images.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(url => url.Trim())
                           .Where(url => !string.IsNullOrWhiteSpace(url))
                           .ToList();
            }
            catch
            {
                // If parsing fails, treat as single URL
                return new List<string> { images.Trim() };
            }
        }

        /* Example
        public async Task<PagedResponse<object>> GetAllOrderAsync(OrderQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.Orders.CountWithSearch(queryParams.Search);
            var orders = await _unitOfWork.Orders.GetOrdersWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "OrderId",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = orders.Select(c =>
                {
                    var ordersResponse = new OrderResponses.OrderGetAllResponses
                    {
                        OrderId = c.OrderId,
                        UserId = c.UserId,
                        OrderDate = c.OrderDate,
                        Status = c.Status
                    };

                    return OrderFieldResponse.SelectFields(ordersResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetOrderById(int id, List<string> selectedFields)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null || order.OrderId == 0)
            {
                return null;
            }

            var orderResponse = new OrderResponses.OrderGetByIdResponses
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status
            };
            return OrderFieldResponse.SelectFields(orderResponse, selectedFields);
        }
        */
    }
}