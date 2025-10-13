using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Apartment;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IApartmentService
    {
        Task<PagedResponse<object>> GetAllAsync(ApartmentQuery queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<int> CreateAsync(ApartmentRequest.ApartmentCreate item);
        Task<bool> UpdateAsync(ApartmentRequest.ApartmentUpdate item, int id);
        Task<bool> DeleteAsync(int id);
    }

    public class ApartmentService : IApartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApartmentField _fieldResponse;
        private readonly IMapper _mapper;
        private readonly ILogger<ApartmentService> _logger;

        public ApartmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ApartmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new ApartmentField();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(ApartmentQuery queryParams)
        {
            // Count total items with search
            var totalItems = await _unitOfWork.Apartments.CountApartmentsWithSearchAsync(
                searchField: queryParams.SearchField,
                search: queryParams.Search
            );

            // Get apartments with advanced query (dynamic search)
            var apartments = await _unitOfWork.Apartments.GetApartmentsWithAdvancedQueryAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,
                search: queryParams.Search,
                sortBy: queryParams.SortBy ?? "ApartmentId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = apartments.Select(a =>
                {
                    var apartmentResponse = _mapper.Map<ApartmentResponse.ApartmentGetAll>(a);
                    return _fieldResponse.SelectFields(apartmentResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var apartment = await _unitOfWork.Apartments.GetByIdWithBuildingAsync(id);
            if (apartment == null || apartment.ApartmentId == 0)
            {
                return null;
            }

            var apartmentResponse = _mapper.Map<ApartmentResponse.ApartmentDetail>(apartment);
            return _fieldResponse.SelectFields(apartmentResponse, selectedFields);
        }

        #region CUD Operations

        public async Task<int> CreateAsync(ApartmentRequest.ApartmentCreate item)
        {
            if (item == null)
            {
                return 0;
            }

            var apartment = _mapper.Map<Apartment>(item);
            apartment.CreatedAt = DateTime.UtcNow;
            
            var result = await _unitOfWork.Apartments.CreateAsync(apartment);
            await _unitOfWork.SaveChangesAsync();
            
            return result > 0 ? apartment.ApartmentId : 0;
        }

        public async Task<bool> UpdateAsync(ApartmentRequest.ApartmentUpdate item, int id)
        {
            if (item == null)
            {
                return false;
            }

            var apartment = await _unitOfWork.Apartments.GetByIdAsync(id);
            if (apartment == null)
            {
                return false;
            }

            apartment.ApartmentCode = item.ApartmentCode;
            apartment.Floor = item.Floor;
            apartment.Area = (decimal?)item.Area;
            apartment.ApartmentType = item.ApartmentType;
            apartment.Status = item.Status;
            apartment.NumberBathroom = item.NumberBathroom;

            if (await _unitOfWork.Apartments.UpdateAsync(apartment) > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var apartment = await _unitOfWork.Apartments.GetByIdAsync(id);
            if (apartment == null || apartment.ApartmentId == 0)
            {
                return false;
            }

            return await _unitOfWork.Apartments.RemoveAsync(apartment);
        }

        #endregion
    }
}
