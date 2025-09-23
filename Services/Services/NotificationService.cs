using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Notification;

namespace Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _unitOfWork.Notifications.GetAllAsync();
        }

        public async Task<PaginationResult<NotificationResponse>> GetAllNotificationsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var notifications = await _unitOfWork.Notifications.GetAllAsync();
            var notificationResponses = notifications.Select(MapToNotificationResponse);
            
            return PaginationResult<NotificationResponse>.Create(notificationResponses, page, pageSize);
        }

        public async Task<Notification?> GetNotificationByIdAsync(int id)
        {
            return await _unitOfWork.Notifications.GetByIdAsync(id);
        }

        public async Task<Notification> CreateNotificationAsync(CreateNotificationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Content = request.Content,
                RelatedId = request.RelatedId,
                CreatedAt = DateTime.Now,
                IsRead = request.IsRead ?? false
            };

            await _unitOfWork.Notifications.CreateAsync(notification);
            return notification;
        }

        public async Task<Notification> UpdateNotificationAsync(int id, UpdateNotificationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                throw new ArgumentException($"Notification with ID {id} not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Type))
                notification.Type = request.Type;
            
            if (!string.IsNullOrEmpty(request.Content))
                notification.Content = request.Content;
            
            if (request.RelatedId.HasValue)
                notification.RelatedId = request.RelatedId;
            
            if (request.IsRead.HasValue)
                notification.IsRead = request.IsRead;

            await _unitOfWork.Notifications.UpdateAsync(notification);
            return notification;
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
                return false;

            return await _unitOfWork.Notifications.RemoveAsync(notification);
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<NotificationResponse>> GetNotificationsByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var userNotifications = allNotifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt);
            var notificationResponses = userNotifications.Select(MapToNotificationResponse);
            
            return PaginationResult<NotificationResponse>.Create(notificationResponses, page, pageSize);
        }

        public async Task<PaginationResult<NotificationResponse>> GetNotificationsByTypeAsync(string type, int page = 1, int pageSize = 10)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var typeNotifications = allNotifications.Where(n => n.Type == type).OrderByDescending(n => n.CreatedAt);
            var notificationResponses = typeNotifications.Select(MapToNotificationResponse);
            
            return PaginationResult<NotificationResponse>.Create(notificationResponses, page, pageSize);
        }

        public async Task<PaginationResult<NotificationResponse>> GetUnreadNotificationsByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var unreadNotifications = allNotifications.Where(n => n.UserId == userId && n.IsRead == false).OrderByDescending(n => n.CreatedAt);
            var notificationResponses = unreadNotifications.Select(MapToNotificationResponse);
            
            return PaginationResult<NotificationResponse>.Create(notificationResponses, page, pageSize);
        }

        public async Task<PaginationResult<NotificationResponse>> SearchNotificationsAsync(NotificationSearchRequest request)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var query = allNotifications.AsQueryable();

            // Apply filters
            if (request.UserId.HasValue)
                query = query.Where(n => n.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(n => n.Type == request.Type);

            if (request.IsRead.HasValue)
                query = query.Where(n => n.IsRead == request.IsRead.Value);

            if (request.FromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(n => n.CreatedAt <= request.ToDate.Value);

            var filteredNotifications = query.OrderByDescending(n => n.CreatedAt);
            var notificationResponses = filteredNotifications.Select(MapToNotificationResponse);

            return PaginationResult<NotificationResponse>.Create(notificationResponses, request.Page, request.PageSize);
        }

        // Status Operations
        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            await _unitOfWork.Notifications.UpdateAsync(notification);
            return true;
        }

        public async Task<bool> MarkNotificationAsUnreadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = false;
            await _unitOfWork.Notifications.UpdateAsync(notification);
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(int userId)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var userNotifications = allNotifications.Where(n => n.UserId == userId && n.IsRead == false);

            foreach (var notification in userNotifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.PrepareUpdate(notification);
            }

            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            return allNotifications.Count(n => n.UserId == userId && n.IsRead == false);
        }

        // Complex Operations
        public async Task<bool> UpdateNotificationStatusAsync(int notificationId, UpdateNotificationStatusRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = request.IsRead;
            await _unitOfWork.Notifications.UpdateAsync(notification);
            return true;
        }

        public async Task<bool> DeleteNotificationsByUserAsync(int userId)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var userNotifications = allNotifications.Where(n => n.UserId == userId);

            foreach (var notification in userNotifications)
            {
                _unitOfWork.Notifications.PrepareRemove(notification);
            }

            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteOldNotificationsAsync(DateTime beforeDate)
        {
            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var oldNotifications = allNotifications.Where(n => n.CreatedAt < beforeDate);

            foreach (var notification in oldNotifications)
            {
                _unitOfWork.Notifications.PrepareRemove(notification);
            }

            var result = await _unitOfWork.SaveChangesAsync();
            return result > 0;
        }

        // Helper method to map Entity → DTO
        private NotificationResponse MapToNotificationResponse(Notification notification)
        {
            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Type = notification.Type,
                Content = notification.Content,
                RelatedId = notification.RelatedId,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                // Note: UserName and UserEmail would require additional queries or joins
                // For performance, these could be loaded separately when needed
                UserName = notification.User?.Username,
                UserEmail = notification.User?.Email
            };
        }
    }
}