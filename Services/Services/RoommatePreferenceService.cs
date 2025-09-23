using Services.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.RoommatePreference;
using VLivingAPI.Repositories.Data.Models;
using EVCS.Repositories.HuyCG.Interfaces;

namespace Services.Services
{
    public class RoommatePreferenceService : IRoommatePreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoommatePreferenceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoommatePreference>> GetAllRoommatePreferencesAsync()
        {
            return await _unitOfWork.RoommatePreferences.GetAllAsync();
        }

        public async Task<PaginationResult<RoommatePreferenceResponse>> GetAllRoommatePreferencesPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var preferences = await _unitOfWork.RoommatePreferences.GetAllAsync();
            var preferenceResponses = preferences.Select(MapToRoommatePreferenceResponse);
            
            return PaginationResult<RoommatePreferenceResponse>.Create(preferenceResponses, page, pageSize);
        }

        public async Task<RoommatePreference?> GetRoommatePreferenceByIdAsync(int id)
        {
            return await _unitOfWork.RoommatePreferences.GetByIdAsync(id);
        }

        public async Task<RoommatePreference> CreateRoommatePreferenceAsync(CreateRoommatePreferenceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate age range
            if (request.AgeRangeMin.HasValue && request.AgeRangeMax.HasValue && 
                request.AgeRangeMin > request.AgeRangeMax)
                throw new ArgumentException("AgeRangeMin cannot be greater than AgeRangeMax");

            // Validate budget range
            if (request.BudgetMin.HasValue && request.BudgetMax.HasValue && 
                request.BudgetMin > request.BudgetMax)
                throw new ArgumentException("BudgetMin cannot be greater than BudgetMax");

            var preference = new RoommatePreference
            {
                UserId = request.UserId,
                PreferredGender = request.PreferredGender,
                AgeRangeMin = request.AgeRangeMin,
                AgeRangeMax = request.AgeRangeMax,
                BudgetMin = request.BudgetMin,
                BudgetMax = request.BudgetMax,
                Habits = request.Habits,
                Interests = request.Interests,
                LocationId = request.LocationId,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.RoommatePreferences.CreateAsync(preference);
            return preference;
        }

        public async Task<RoommatePreference> UpdateRoommatePreferenceAsync(int id, UpdateRoommatePreferenceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingPreference = await _unitOfWork.RoommatePreferences.GetByIdAsync(id);
            if (existingPreference == null)
                throw new KeyNotFoundException($"RoommatePreference with id {id} not found");

            // Validate age range
            if (request.AgeRangeMin.HasValue && request.AgeRangeMax.HasValue && 
                request.AgeRangeMin > request.AgeRangeMax)
                throw new ArgumentException("AgeRangeMin cannot be greater than AgeRangeMax");

            // Validate budget range
            if (request.BudgetMin.HasValue && request.BudgetMax.HasValue && 
                request.BudgetMin > request.BudgetMax)
                throw new ArgumentException("BudgetMin cannot be greater than BudgetMax");

            // Update properties
            existingPreference.PreferredGender = request.PreferredGender ?? existingPreference.PreferredGender;
            existingPreference.AgeRangeMin = request.AgeRangeMin ?? existingPreference.AgeRangeMin;
            existingPreference.AgeRangeMax = request.AgeRangeMax ?? existingPreference.AgeRangeMax;
            existingPreference.BudgetMin = request.BudgetMin ?? existingPreference.BudgetMin;
            existingPreference.BudgetMax = request.BudgetMax ?? existingPreference.BudgetMax;
            existingPreference.Habits = request.Habits ?? existingPreference.Habits;
            existingPreference.Interests = request.Interests ?? existingPreference.Interests;
            existingPreference.LocationId = request.LocationId ?? existingPreference.LocationId;
            existingPreference.UpdatedAt = DateTime.Now;

            await _unitOfWork.RoommatePreferences.UpdateAsync(existingPreference);
            return existingPreference;
        }

        public async Task<bool> DeleteRoommatePreferenceAsync(int id)
        {
            var preference = await _unitOfWork.RoommatePreferences.GetByIdAsync(id);
            if (preference == null)
                return false;

            return await _unitOfWork.RoommatePreferences.RemoveAsync(preference);
        }

        public async Task<RoommatePreference?> GetRoommatePreferenceByUserIdAsync(int userId)
        {
            var preferences = await _unitOfWork.RoommatePreferences.GetAllAsync();
            return preferences.FirstOrDefault(p => p.UserId == userId);
        }

        public async Task<PaginationResult<RoommatePreferenceResponse>> GetRoommatePreferencesByLocationAsync(int locationId, int page = 1, int pageSize = 10)
        {
            var preferences = await _unitOfWork.RoommatePreferences.GetAllAsync();
            var filteredPreferences = preferences.Where(p => p.LocationId == locationId);
            var preferenceResponses = filteredPreferences.Select(MapToRoommatePreferenceResponse);
            
            return PaginationResult<RoommatePreferenceResponse>.Create(preferenceResponses, page, pageSize);
        }

        // Helper method to map Entity to DTO
        private RoommatePreferenceResponse MapToRoommatePreferenceResponse(RoommatePreference preference)
        {
            return new RoommatePreferenceResponse
            {
                PreferenceId = preference.PreferenceId,
                UserId = preference.UserId,
                PreferredGender = preference.PreferredGender,
                AgeRangeMin = preference.AgeRangeMin,
                AgeRangeMax = preference.AgeRangeMax,
                BudgetMin = preference.BudgetMin,
                BudgetMax = preference.BudgetMax,
                Habits = preference.Habits,
                Interests = preference.Interests,
                LocationId = preference.LocationId,
                UpdatedAt = preference.UpdatedAt,
                // Optional: populate related data if needed
                UserName = preference.User?.FullName,
                LocationName = preference.Location?.Name
            };
        }
    }
}