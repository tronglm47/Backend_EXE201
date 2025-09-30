using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Amenity;
using Services.RequestsResponses.PostAmenity;
using Services.RequestsResponses.PropertyForm;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IPostAmenityService
    {
        Task<PagedResponse<object>> GetAllAsync(PostAmenityQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(PostAmenityRequest.CreatePostAmenity item);
        Task<bool> Delete(int id);
        Task<bool> Update(PostAmenityRequest.UpdatePostAmenity item, int id);
    }
    public class PostAmenityService : IPostAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AmenityFieldResponse _fieldResponse;

        private readonly IMapper _mapper;
        private readonly ILogger<PostAmenityService> _logger;
        public PostAmenityService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PostAmenityService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new AmenityFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(PostAmenityQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.PostAmenity.CountWithSearch(queryParams.Search);
            var objects = await _unitOfWork.PostAmenity.GetPostAmenityWithAdvancedQuery(
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
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<PostAmenityResponse.GetAll>(c);

                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var item = await _unitOfWork.PostAmenity.GetByIdAsync(id);
            if (item == null || item.PostId == 0 || item.AmenityId == 0)
            {
                return null;
            }

            var itemResponse = _mapper.Map<PostAmenityResponse.GetById>(item);
            return _fieldResponse.SelectFields(item, selectedFields);
        }
        #region CUD Operations
        // Implement Create, Update, Delete methods if needed

        public async Task<int> Create(PostAmenityRequest.CreatePostAmenity item)
        {
            if (item == null)
            {
                return 0;
            }
            var postAmenity = _mapper.Map<PostAmenity>(item);
            var result = await _unitOfWork.PostAmenity.CreateAsync(postAmenity);
            await _unitOfWork.SaveChangesAsync();
            return result > 0 ? postAmenity.PostId : 0;
        }
        public async Task<bool> Delete(int id)
        {
            var item = await _unitOfWork.PostAmenity.GetByIdAsync(id);
            if (item == null || item.PostId == 0 || item.AmenityId == 0)
            {
                return false;
            }
            return await _unitOfWork.PostAmenity.RemoveAsync(item);
        }
        public async Task<bool> Update(PostAmenityRequest.UpdatePostAmenity item, int id)
        {
            if (item == null)
            {
                return false;
            }
            var postAmenity = await _unitOfWork.PostAmenity.GetByIdAsync(id);
            if (postAmenity == null)
            {
                return false;
            }
            postAmenity.PostId = item.PostID;
            postAmenity.AmenityId = item.AmenityId;
            postAmenity.Notes = item.Notes;
            if (await _unitOfWork.PostAmenity.UpdateAsync(postAmenity) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
