using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Post;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;

namespace Services.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region CRUD Operations

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _unitOfWork.Posts.GetAllAsync();
        }

        public async Task<PaginationResult<PostResponse>> GetAllPostsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var posts = await _unitOfWork.Posts.GetAllAsync();
            var postResponses = new List<PostResponse>();

            foreach (var post in posts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        public async Task<Post?> GetPostByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid post ID");

            return await _unitOfWork.Posts.GetByIdAsync(id);
        }

        public async Task<PostResponse?> GetPostDetailsByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid post ID");

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
                return null;

            return await MapToPostResponseWithDetailsAsync(post);
        }

        public async Task<Post> CreatePostAsync(CreatePostRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var post = new Post
            {
                UserId = request.UserId,
                PropertyId = request.PropertyId,
                Type = request.Type,
                Title = request.Title,
                Content = request.Content,
                Images = request.Images,
                CreatedAt = DateTime.Now,
                Views = 0
            };

            await _unitOfWork.Posts.CreateAsync(post);
            return post;
        }

        public async Task<Post> UpdatePostAsync(int id, UpdatePostRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
                throw new ArgumentException("Post not found");

            // Update only provided fields
            if (request.PropertyId.HasValue)
                post.PropertyId = request.PropertyId;
            
            if (!string.IsNullOrEmpty(request.Type))
                post.Type = request.Type;
            
            if (!string.IsNullOrEmpty(request.Title))
                post.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Content))
                post.Content = request.Content;
            
            if (request.Images != null)
                post.Images = request.Images;

            await _unitOfWork.Posts.UpdateAsync(post);
            return post;
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid post ID");

            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null)
                return false;

            return await _unitOfWork.Posts.RemoveAsync(post);
        }

        #endregion

        #region Business Operations

        public async Task<PaginationResult<PostResponse>> GetPostsByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var userPosts = allPosts.Where(p => p.UserId == userId);
            var postResponses = new List<PostResponse>();

            foreach (var post in userPosts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        public async Task<PaginationResult<PostResponse>> GetPostsByPropertyAsync(int propertyId, int page = 1, int pageSize = 10)
        {
            if (propertyId <= 0)
                throw new ArgumentException("Invalid property ID");

            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var propertyPosts = allPosts.Where(p => p.PropertyId == propertyId);
            var postResponses = new List<PostResponse>();

            foreach (var post in propertyPosts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        public async Task<PaginationResult<PostResponse>> GetPostsByTypeAsync(string type, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type is required");

            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var typedPosts = allPosts.Where(p => p.Type.ToLower() == type.ToLower());
            var postResponses = new List<PostResponse>();

            foreach (var post in typedPosts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        public async Task<PaginationResult<PostResponse>> SearchPostsAsync(PostSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Page < 1 || request.PageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var filteredPosts = allPosts.AsQueryable();

            // Apply filters
            if (request.UserId.HasValue)
                filteredPosts = filteredPosts.Where(p => p.UserId == request.UserId.Value);

            if (request.PropertyId.HasValue)
                filteredPosts = filteredPosts.Where(p => p.PropertyId == request.PropertyId.Value);

            if (!string.IsNullOrWhiteSpace(request.Type))
                filteredPosts = filteredPosts.Where(p => p.Type.ToLower().Contains(request.Type.ToLower()));

            if (!string.IsNullOrWhiteSpace(request.Title))
                filteredPosts = filteredPosts.Where(p => p.Title.ToLower().Contains(request.Title.ToLower()));

            if (!string.IsNullOrWhiteSpace(request.Content))
                filteredPosts = filteredPosts.Where(p => p.Content.ToLower().Contains(request.Content.ToLower()));

            if (request.CreatedAfter.HasValue)
                filteredPosts = filteredPosts.Where(p => p.CreatedAt >= request.CreatedAfter.Value);

            if (request.CreatedBefore.HasValue)
                filteredPosts = filteredPosts.Where(p => p.CreatedAt <= request.CreatedBefore.Value);

            if (request.MinViews.HasValue)
                filteredPosts = filteredPosts.Where(p => p.Views >= request.MinViews.Value);

            if (request.MaxViews.HasValue)
                filteredPosts = filteredPosts.Where(p => p.Views <= request.MaxViews.Value);

            var posts = filteredPosts.ToList();
            var postResponses = new List<PostResponse>();

            foreach (var post in posts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, request.Page, request.PageSize);
        }

        #endregion

        #region View Operations

        public async Task<bool> IncrementPostViewsAsync(int postId)
        {
            if (postId <= 0)
                throw new ArgumentException("Invalid post ID");

            var post = await _unitOfWork.Posts.GetByIdAsync(postId);
            if (post == null)
                return false;

            post.Views = (post.Views ?? 0) + 1;
            await _unitOfWork.Posts.UpdateAsync(post);
            return true;
        }

        public async Task<PaginationResult<PostResponse>> GetPopularPostsAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var popularPosts = allPosts.OrderByDescending(p => p.Views ?? 0).ToList();
            var postResponses = new List<PostResponse>();

            foreach (var post in popularPosts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        public async Task<PaginationResult<PostResponse>> GetRecentPostsAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            var recentPosts = allPosts.OrderByDescending(p => p.CreatedAt ?? DateTime.MinValue).ToList();
            var postResponses = new List<PostResponse>();

            foreach (var post in recentPosts)
            {
                postResponses.Add(await MapToPostResponseWithDetailsAsync(post));
            }

            return PaginationResult<PostResponse>.Create(postResponses, page, pageSize);
        }

        #endregion

        #region Statistics Operations

        public async Task<int> GetPostCountByUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            return allPosts.Count(p => p.UserId == userId);
        }

        public async Task<int> GetPostCountByPropertyAsync(int propertyId)
        {
            if (propertyId <= 0)
                throw new ArgumentException("Invalid property ID");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            return allPosts.Count(p => p.PropertyId == propertyId);
        }

        public async Task<int> GetTotalViewsByUserAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            var allPosts = await _unitOfWork.Posts.GetAllAsync();
            return allPosts.Where(p => p.UserId == userId).Sum(p => p.Views ?? 0);
        }

        #endregion

        #region Helper Methods

        private PostResponse MapToPostResponse(Post post)
        {
            return new PostResponse
            {
                PostId = post.PostId,
                UserId = post.UserId,
                PropertyId = post.PropertyId,
                Type = post.Type,
                Title = post.Title,
                Content = post.Content,
                Images = post.Images,
                CreatedAt = post.CreatedAt,
                Views = post.Views
            };
        }

        private async Task<PostResponse> MapToPostResponseWithDetailsAsync(Post post)
        {
            var response = MapToPostResponse(post);

            // Get user information
            if (post.User != null)
            {
                response.UserName = post.User.Username;
            }
            else
            {
                var allUsers = await _unitOfWork.Repository<User>().GetAllAsync();
                var user = allUsers.FirstOrDefault(u => u.UserId == post.UserId);
                response.UserName = user?.Username;
            }

            // Get property information
            if (post.PropertyId.HasValue)
            {
                if (post.Property != null)
                {
                    response.PropertyTitle = post.Property.Description; // Using Description as title since Title doesn't exist
                    response.PropertyType = post.Property.Type;
                }
                else
                {
                    var allProperties = await _unitOfWork.Repository<Property>().GetAllAsync();
                    var property = allProperties.FirstOrDefault(p => p.PropertyId == post.PropertyId.Value);
                    response.PropertyTitle = property?.Description; // Using Description as title since Title doesn't exist
                    response.PropertyType = property?.Type;
                }
            }

            // Count bookings related to this post
            var allBookings = await _unitOfWork.Repository<Booking>().GetAllAsync();
            response.BookingsCount = allBookings.Count(b => b.PostId == post.PostId);

            return response;
        }

        #endregion
    }
}