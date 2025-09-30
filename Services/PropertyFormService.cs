using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.PropertyForm;
using Services.RequestsResponses.PropertyType;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPropertyFormService
    {
        Task<PagedResponse<object>> GetAllAsync(PropertyFormQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(PropertyFormRequest.CreatePropertyForm item);
        Task<bool> Delete(int id);
        Task<bool> Update(PropertyFormRequest.UpdatePropertyForm item, int id);
    }
    public class PropertyFormService : IPropertyFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PropertyFormFieldResponse _fieldResponse;

        private readonly IMapper _mapper;
        private readonly ILogger<PropertyFormService> _logger;
        public PropertyFormService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PropertyFormService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new PropertyFormFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(PropertyFormQueryParameters queryParams)
        {
            var _repository = _unitOfWork.PropertyForms as PropertyFormRepository;
            if (_repository == null)
                throw new InvalidOperationException("Repository not available");

            var totalItems = await _repository.CountWithSearch(queryParams.Search);
            var objects = await _repository.GetPropertyFormsWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "PropertyFormId",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<PropertyFormResponse.PropertyFormGetAllResponse>(c);

                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var item = await _unitOfWork.PropertyForms.GetByIdAsync(id);
            if (item == null || item.PropertyFormId == 0)
            {
                return null;
            }

            var itemResponse = _mapper.Map<PropertyFormResponse.PropertyFormGetByIdResponse>(item);
            return _fieldResponse.SelectFields(item, selectedFields);
        }
        #region CUD Operations
        // Implement Create, Update, Delete methods if needed

        public async Task<int> Create(PropertyFormRequest.CreatePropertyForm item)
        {
            if (item == null)
            {
                return 0;
            }
            var propertyForm = _mapper.Map<PropertyForm>(item);
            var result = await _unitOfWork.PropertyForms.CreateAsync(propertyForm);
            await _unitOfWork.SaveChangesAsync();
            return result > 0 ? propertyForm.PropertyFormId : 0;
        }
        public async Task<bool> Delete(int id)
        {
            var item = await _unitOfWork.PropertyForms.GetByIdAsync(id);
            if (item == null || item.PropertyFormId == 0)
            {
                return false;
            }
            return await _unitOfWork.PropertyForms.RemoveAsync(item);
        }
        public async Task<bool> Update(PropertyFormRequest.UpdatePropertyForm item, int id)
        {
            if (item == null)
            {
                return false;
            }
            var propertyForm = await _unitOfWork.PropertyForms.GetByIdAsync(id);
            if (propertyForm == null)
            {
                return false;
            }
            propertyForm.Name = item.Name;
            propertyForm.Description = item.Description;
            if (await _unitOfWork.PropertyForms.UpdateAsync(propertyForm) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion


    }
}
