using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Services.RequestsResponses;
using Services.RequestsResponses.Utility;

namespace Services
{
    public interface IUtilityService
    {
        Task<PagedResponse<object>> GetAllAsync(UtilityQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<int> CreateAsync(UtilityRequest.UtilityCreate item);
        Task<bool> UpdateAsync(UtilityRequest.UtilityUpdate item, int id);
        Task<bool> DeleteAsync(int id);
    }
    public class UtilityService : IUtilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UtilityService> _logger;
        private readonly UtilityField _fieldResponse;
        private readonly IMapper _mapper;
        public UtilityService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UtilityService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new UtilityField();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(UtilityQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.Utilities.CountUtilitiesWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get utilities with advanced query (dynamic search)
            var utilities = await _unitOfWork.Utilities.GetUtilitiesWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,  // Dynamic search field
                search: queryParams.Search,            // Dynamic search value
                sortBy: queryParams.SortBy ?? "UtilityId",
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
                    var utilityResponse = _mapper.Map<UtilityResponse.UtilityGetAll>(s);
                    return _fieldResponse.SelectFields(utilityResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var utility = await _unitOfWork.Utilities.GetByIdAsync(id);
            if (utility == null || utility.UtilityId == 0)
            {
                return null;
            }

            var utilityResponse = _mapper.Map<UtilityResponse.UtilityDetail>(utility);
            return _fieldResponse.SelectFields(utilityResponse, selectedFields);
        }

        #region CUD Operations

        public async Task<int> CreateAsync(UtilityRequest.UtilityCreate item)
        {
            if (item == null)
            {
                return 0;
            }

            var utility = _mapper.Map<Utility>(item);
            utility.CreatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.Utilities.CreateAsync(utility);
            await _unitOfWork.SaveChangesAsync();

            return result > 0 ? utility.UtilityId : 0;
        }

        public async Task<bool> UpdateAsync(UtilityRequest.UtilityUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }

            var utility = await _unitOfWork.Utilities.GetByIdAsync(id);
            if (utility == null)
            {
                return false;
            }

            utility.Name = item.Name;

            if (await _unitOfWork.Utilities.UpdateAsync(utility) > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var utility = await _unitOfWork.Utilities.GetByIdAsync(id);
            if (utility == null || utility.UtilityId == 0)
            {
                return false;
            }

            return await _unitOfWork.Utilities.RemoveAsync(utility);
        }

        #endregion
    }
}
