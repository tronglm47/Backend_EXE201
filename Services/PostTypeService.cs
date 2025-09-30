using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.PostAmenity;
using Services.RequestsResponses.PostType;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPostTypeService
    {
        Task<PagedResponse<object>> GetAllAsync(PostTypeQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(PostTypeRequest.PostTypeCreate item);
        Task<bool> Delete(int id);
        Task<bool> Update(PostTypeRequest.PostTypeUpdate item, int id);
    }
    public class PostTypeService : IPostTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PostTypeFieldResponse _fieldResponse;

        private readonly IMapper _mapper;
        private readonly ILogger<AmenityService> _logger;
        public PostTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AmenityService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new PostTypeFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(PostTypeQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.PostType.CountWithSearch(queryParams.Search);
            var objects = await _unitOfWork.PostType.GetPostTypesWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "posttypeid",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<PostTypeResponse.PostTypeGetAll>(c);

                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var item = await _unitOfWork.PostType.GetByIdAsync(id);
            if (item == null || item.PostTypeId == 0)
            {
                return null;
            }

            var itemResponse = _mapper.Map<PostTypeResponse.PostTypeGetById>(item);
            return _fieldResponse.SelectFields(item, selectedFields);
        }
        #region CUD Operations
        // Implement Create, Update, Delete methods if needed

        public async Task<int> Create(PostTypeRequest.PostTypeCreate item)
        {
            if (item == null)
            {
                return 0;
            }
            var postType = _mapper.Map<PostType>(item);
            var result = await _unitOfWork.PostType.CreateAsync(postType);
            await _unitOfWork.SaveChangesAsync();
            return result > 0 ? postType.PostTypeId : 0;
        }
        public async Task<bool> Delete(int id)
        {
            var item = await _unitOfWork.PostType.GetByIdAsync(id);
            if (item == null || item.PostTypeId == 0)
            {
                return false;
            }
            return await _unitOfWork.PostType.RemoveAsync(item);
        }
        public async Task<bool> Update(PostTypeRequest.PostTypeUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }
            var postType = await _unitOfWork.PostType.GetByIdAsync(id);
            if (postType == null)
            {
                return false;
            }
            postType.Name = item.Name;
            postType.Description = item.Description;
            if (await _unitOfWork.PostType.UpdateAsync(postType) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
