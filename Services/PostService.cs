using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Post;
using Repositories;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPostService
    {
        Task<PagedResponse<object>> GetAllPostAsync(PostQueryParameters queryParams);
        Task<object?> GetPostById(int id, List<string> selectedFields);
    }
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PostFieldResponse _postFieldResponse;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _postFieldResponse = new PostFieldResponse();
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
            var post = await _unitOfWork.Posts.GetByIdAsync(id);
            if (post == null || post.PostId == 0)
            {
                return null;
            }

            var postResponse = new PostResponse.PostGetByIdResponse
            {
                PostId = post.PostId,
                UserId = post.UserId,
                PostTypeId = post.PostTypeId,
                PropertyTypeId = post.PropertyTypeId,
                PropertyFormId = post.PropertyFormId,
                LocationId = post.LocationId,
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