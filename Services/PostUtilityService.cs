using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Services.RequestsResponses;
using Services.RequestsResponses.PostUtility;

namespace Services
{
    public interface IPostUtilityService
    {
        Task<PagedResponse<object>> GetAllAsync(PostUtilityQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<int> CreateAsync(PostUtilityRequest.PostUtilityCreate item);
        Task<bool> UpdateAsync(PostUtilityRequest.PostUtilityUpdate item, int id);
        Task<bool> DeleteAsync(int id);
    }
    public class PostUtilityService : IPostUtilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostUtilityService> _logger;
        private readonly PostUtilityField _fieldResponse;
        private readonly IMapper _mapper;
        public PostUtilityService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PostUtilityService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new PostUtilityField();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(PostUtilityQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.PostUtilities.CountPostUtilitiesWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get utilities with advanced query (dynamic search)
            var utilities = await _unitOfWork.PostUtilities.GetPostUtilitiesWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,  // Dynamic search field
                search: queryParams.Search,            // Dynamic search value
                sortBy: queryParams.SortBy ?? "postId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = utilities.Select(s =>
                {
                    var utilityResponse = _mapper.Map<PostUtilityResponse.PostUtilityGetAll>(s);
                    return _fieldResponse.SelectFields(utilityResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var utility = await _unitOfWork.PostUtilities.GetByIdAsync(id);
            if (utility == null || utility.PostId == 0)
            {
                return null;
            }

            var utilityResponse = _mapper.Map<PostUtilityResponse.PostUtilityDetail>(utility);
            return _fieldResponse.SelectFields(utilityResponse, selectedFields);
        }

        #region CUD Operations

        public async Task<int> CreateAsync(PostUtilityRequest.PostUtilityCreate item)
        {
            if (item == null)
            {
                return 0;
            }

            var utility = _mapper.Map<PostUtility>(item);

            var result = await _unitOfWork.PostUtilities.CreateAsync(utility);
            await _unitOfWork.SaveChangesAsync();

            return result > 0 ? utility.PostId : 0;
        }

        public async Task<bool> UpdateAsync(PostUtilityRequest.PostUtilityUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }

            var utility = await _unitOfWork.PostUtilities.GetByIdAsync(id);
            if (utility == null)
            {
                return false;
            }

            utility.Notes = item.Note ?? utility.Notes;

            if (await _unitOfWork.PostUtilities.UpdateAsync(utility) > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var utility = await _unitOfWork.PostUtilities.GetByIdAsync(id);
            if (utility == null || utility.PostId == 0)
            {
                return false;
            }

            return await _unitOfWork.PostUtilities.RemoveAsync(utility);
        }

        #endregion
    }
}
