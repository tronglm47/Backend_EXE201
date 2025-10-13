using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.RequestsResponses;
using Services.RequestsResponses.User;
using Repositories.Models;

namespace Services
{
    public interface IUserService
    {
        Task<PagedResponse<object>> GetAllAsync(UserQueryParameters queryParams);
        Task<object?> GetByIdAsync(int id, List<string> selectedFields);
        Task<bool> UpdateAsync(UserRequest.AdminUpdateUserRequest request, int id);
        Task<bool> DeleteAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserFieldResponse _fieldResponse;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _fieldResponse = new UserFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResponse<object>> GetAllAsync(UserQueryParameters queryParams)
        {
            // Count total items with search
            var totalItems = await _userRepository.CountUsersAsync(
                searchField: queryParams.SearchField,
                searchValue: queryParams.Search
            );

            // Get users with advanced query (dynamic search)
            var users = await _userRepository.GetAllUsersAsync(
                page: queryParams.Page,
                pageSize: queryParams.PageSize,
                searchField: queryParams.SearchField,
                searchValue: queryParams.Search,
                sortBy: queryParams.SortBy ?? "UserId",
                isDescending: queryParams.IsDescending
            );

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = users.Select(u =>
                {
                    var userResponse = _mapper.Map<UserResponse.UserGetAll>(u);
                    return _fieldResponse.SelectFields(userResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetByIdAsync(int id, List<string> selectedFields)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            var userResponse = _mapper.Map<UserResponse.UserDetail>(user);
            return _fieldResponse.SelectFields(userResponse, selectedFields);
        }

        public async Task<bool> UpdateAsync(UserRequest.AdminUpdateUserRequest request, int id)
        {
            if (request == null)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // Check if username is being changed and if it's already taken by another user
            if (request.Username != user.Username)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
                if (existingUser != null && existingUser.UserId != id)
                {
                    _logger.LogWarning("Username {Username} is already taken by another user", request.Username);
                    return false;
                }
            }

            // Check if email is being changed and if it's already taken by another user
            if (request.Email != user.Email)
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null && existingUser.UserId != id)
                {
                    _logger.LogWarning("Email {Email} is already taken by another user", request.Email);
                    return false;
                }
            }

            // Update user properties (excluding password)
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            user.Bio = request.Bio;

            return await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return false;
            }

            return await _userRepository.DeleteUserAsync(id);
        }
    }
}
