using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.PropertyType;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPropertyTypeService
    {
        Task<PagedResponse<object>> GetAllAsync(PropertyTypeQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(PropertyTypeRequest.CreateRequest item);
        Task<bool> Delete(int id);
        Task<bool> Update(PropertyTypeRequest.UpdateRequest item, int id);
    }
    public class PropertyTypeService : IPropertyTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PropertyTypeFieldResponse _fieldResponse;

        private readonly IMapper _mapper;
        private readonly ILogger<PropertyTypeService> _logger;
        public PropertyTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PropertyTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new PropertyTypeFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(PropertyTypeQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.PropertyTypes.CountWithSearch(queryParams.Search);
            var objects = await _unitOfWork.PropertyTypes.GetPropertyTypesWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "PropertyTypeId",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<PropertyTypeResponse.PropertyTypeGetAllResponse>(c);

                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var item = await _unitOfWork.PropertyTypes.GetByIdAsync(id);
            if (item == null || item.PropertyTypeId == 0)
            {
                return null;
            }

            var itemResponse = _mapper.Map<PropertyTypeResponse.PropertyTypeGetByIdResponse>(item);
            return _fieldResponse.SelectFields(item, selectedFields);
        }
        #region CUD Operations
        // Implement Create, Update, Delete methods if needed

        public async Task<int> Create(PropertyTypeRequest.CreateRequest item)
        {
            if (item == null)
            {
                return 0;
            }
            var propertyType = _mapper.Map<PropertyType>(item);
            var result = await _unitOfWork.PropertyTypes.CreateAsync(propertyType);
            await _unitOfWork.SaveChangesAsync();
            return result > 0 ? propertyType.PropertyTypeId : 0;
        }
        public async Task<bool> Delete(int id)
        {
            var item = await _unitOfWork.PropertyTypes.GetByIdAsync(id);
            if (item == null || item.PropertyTypeId == 0)
            {
                return false;
            }
            return await _unitOfWork.PropertyTypes.RemoveAsync(item);
        }
        public async Task<bool> Update(PropertyTypeRequest.UpdateRequest item, int id)
        {
            if (item == null)
            {
                return false;
            }
            var propertityType = await _unitOfWork.PropertyTypes.GetByIdAsync(id);
            if (propertityType == null)
            {
                return false;
            }
            propertityType.Name = item.Name;
            propertityType.Description = item.Description;
            if (await _unitOfWork.PropertyTypes.UpdateAsync(propertityType) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
