using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Notification;

namespace Services.Interfaces
{
    public interface INotificationService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Notification>> GetAllNotificationsAsync();
        Task<PaginationResult<NotificationResponse>> GetAllNotificationsPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Notification?> GetNotificationByIdAsync(int id);
        Task<Notification> CreateNotificationAsync(CreateNotificationRequest request);
        Task<Notification> UpdateNotificationAsync(int id, UpdateNotificationRequest request);
        Task<bool> DeleteNotificationAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<NotificationResponse>> GetNotificationsByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<NotificationResponse>> GetNotificationsByTypeAsync(string type, int page = 1, int pageSize = 10);
        Task<PaginationResult<NotificationResponse>> GetUnreadNotificationsByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<NotificationResponse>> SearchNotificationsAsync(NotificationSearchRequest request);
        
        // Status Operations
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> MarkNotificationAsUnreadAsync(int notificationId);
        Task<bool> MarkAllNotificationsAsReadAsync(int userId);
        Task<int> GetUnreadNotificationCountAsync(int userId);
        
        // Complex Operations
        Task<bool> UpdateNotificationStatusAsync(int notificationId, UpdateNotificationStatusRequest request);
        Task<bool> DeleteNotificationsByUserAsync(int userId);
        Task<bool> DeleteOldNotificationsAsync(DateTime beforeDate);
    }
}