using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Post;

namespace Services.Interfaces
{
    public interface IPostService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<PaginationResult<PostResponse>> GetAllPostsPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Post?> GetPostByIdAsync(int id);
        Task<PostResponse?> GetPostDetailsByIdAsync(int id);
        Task<Post> CreatePostAsync(CreatePostRequest request);
        Task<Post> UpdatePostAsync(int id, UpdatePostRequest request);
        Task<bool> DeletePostAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<PostResponse>> GetPostsByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<PostResponse>> GetPostsByPropertyAsync(int propertyId, int page = 1, int pageSize = 10);
        Task<PaginationResult<PostResponse>> GetPostsByTypeAsync(string type, int page = 1, int pageSize = 10);
        Task<PaginationResult<PostResponse>> SearchPostsAsync(PostSearchRequest request);

        // View Operations
        Task<bool> IncrementPostViewsAsync(int postId);
        Task<PaginationResult<PostResponse>> GetPopularPostsAsync(int page = 1, int pageSize = 10);
        Task<PaginationResult<PostResponse>> GetRecentPostsAsync(int page = 1, int pageSize = 10);

        // Statistics Operations
        Task<int> GetPostCountByUserAsync(int userId);
        Task<int> GetPostCountByPropertyAsync(int propertyId);
        Task<int> GetTotalViewsByUserAsync(int userId);
    }
}