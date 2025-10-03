using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Subdivision;
using Repositories.Models;

namespace Services
{
    public interface ISubdivisionService
    {
        Task<PagedResponse<object>> GetAllAsync(SubdivisionQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<int> CreateAsync(SubdivisionRequest.SubdivisionCreate item);
        Task<bool> UpdateAsync(SubdivisionRequest.SubdivisionUpdate item, int id);
        Task<bool> DeleteAsync(int id);
    }

    public class SubdivisionService : ISubdivisionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SubdivisionField _fieldResponse;
        private readonly IMapper _mapper;
        private readonly ILogger<SubdivisionService> _logger;

        public SubdivisionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SubdivisionService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new SubdivisionField();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(SubdivisionQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.Subdivisions.CountSubdivisionsWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get subdivisions with advanced query (dynamic search)
            var subdivisions = await _unitOfWork.Subdivisions.GetSubdivisionsWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,  // Dynamic search field
                search: queryParams.Search,            // Dynamic search value
                sortBy: queryParams.SortBy ?? "SubdivisionId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = subdivisions.Select(s =>
                {
                    var subdivisionResponse = _mapper.Map<SubdivisionResponse.SubdivisionGetAll>(s);
                    return _fieldResponse.SelectFields(subdivisionResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var subdivision = await _unitOfWork.Subdivisions.GetByIdAsync(id);
            if (subdivision == null || subdivision.SubdivisionId == 0)
            {
                return null;
            }

            var subdivisionResponse = _mapper.Map<SubdivisionResponse.SubdivisionDetail>(subdivision);
            return _fieldResponse.SelectFields(subdivisionResponse, selectedFields);
        }

        #region CUD Operations

        public async Task<int> CreateAsync(SubdivisionRequest.SubdivisionCreate item)
        {
            if (item == null)
            {
                return 0;
            }

            var subdivision = _mapper.Map<Subdivision>(item);
            subdivision.CreatedAt = DateTime.UtcNow;
            
            var result = await _unitOfWork.Subdivisions.CreateAsync(subdivision);
            await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? subdivision.SubdivisionId : 0;
        }

        public async Task<bool> UpdateAsync(SubdivisionRequest.SubdivisionUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }

            var subdivision = await _unitOfWork.Subdivisions.GetByIdAsync(id);
            if (subdivision == null)
            {
                return false;
            }

            subdivision.Name = item.name;
            subdivision.Description = item.description;

            if (await _unitOfWork.Subdivisions.UpdateAsync(subdivision) > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subdivision = await _unitOfWork.Subdivisions.GetByIdAsync(id);
            if (subdivision == null || subdivision.SubdivisionId == 0)
            {
                return false;
            }

            return await _unitOfWork.Subdivisions.RemoveAsync(subdivision);
        }

        #endregion
    }
}
