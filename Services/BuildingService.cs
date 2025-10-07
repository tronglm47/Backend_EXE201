using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Building;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IBuildingService
    {
        Task<PagedResponse<object>> GetAllAsync(BuildingQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<int> CreateAsync(BuildingRequest.BuildingCreate item);
        Task<bool> UpdateAsync(BuildingRequest.BuildingUpdate item, int id);
        Task<bool> DeleteAsync(int id);
    }

    public class BuildingService : IBuildingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BuildingField _fieldResponse;
        private readonly IMapper _mapper;
        private readonly ILogger<BuildingService> _logger;

        public BuildingService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BuildingService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new BuildingField();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(BuildingQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.Buildings.CountBuildingsWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get buildings with advanced query (dynamic search)
            var buildings = await _unitOfWork.Buildings.GetBuildingsWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,
                search: queryParams.Search,
                sortBy: queryParams.SortBy ?? "BuildingId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = buildings.Select(b =>
                {
                    var buildingResponse = _mapper.Map<BuildingResponse.BuildingGetAll>(b);
                    return _fieldResponse.SelectFields(buildingResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var building = await _unitOfWork.Buildings.GetByIdWithSubdivisionAsync(id);
            if (building == null || building.BuildingId == 0)
            {
                return null;
            }

            var buildingResponse = _mapper.Map<BuildingResponse.BuildingDetail>(building);
            return _fieldResponse.SelectFields(buildingResponse, selectedFields);
        }

        #region CUD Operations

        public async Task<int> CreateAsync(BuildingRequest.BuildingCreate item)
        {
            if (item == null)
            {
                return 0;
            }

            var building = _mapper.Map<Building>(item);
            building.CreatedAt = DateTime.UtcNow;
            
            var result = await _unitOfWork.Buildings.CreateAsync(building);
            await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? building.BuildingId : 0;
        }

        public async Task<bool> UpdateAsync(BuildingRequest.BuildingUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }

            var building = await _unitOfWork.Buildings.GetByIdAsync(id);
            if (building == null)
            {
                return false;
            }

            building.SubdivisionId = item.SubdivisionId;
            building.Name = item.Name;
            building.BlockCode = item.BlockCode;

            if (await _unitOfWork.Buildings.UpdateAsync(building) > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(id);
            if (building == null || building.BuildingId == 0)
            {
                return false;
            }

            return await _unitOfWork.Buildings.RemoveAsync(building);
        }

        #endregion
    }
}
