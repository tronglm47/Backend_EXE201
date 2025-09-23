using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.Notification;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Notification là thông tin cá nhân, cần đăng nhập
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get all notifications with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _notificationService.GetAllNotificationsPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, data = notification });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdNotification = await _notificationService.CreateNotificationAsync(request);
                return CreatedAtAction(nameof(GetNotification), new { id = createdNotification.NotificationId }, 
                    new { success = true, data = createdNotification });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update notification
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, [FromBody] UpdateNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedNotification = await _notificationService.UpdateNotificationAsync(id, request);
                return Ok(new { success = true, data = updatedNotification });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting notification: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get notifications by user ID with pagination
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotificationsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _notificationService.GetNotificationsByUserAsync(userId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get unread notifications by user ID with pagination
        /// </summary>
        [HttpGet("user/{userId}/unread")]
        public async Task<IActionResult> GetUnreadNotificationsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _notificationService.GetUnreadNotificationsByUserAsync(userId, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get unread notification count for a user
        /// </summary>
        [HttpGet("user/{userId}/unread-count")]
        public async Task<IActionResult> GetUnreadNotificationCount(int userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Ok(new { success = true, data = new { userId = userId, unreadCount = count } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get notifications by type with pagination
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetNotificationsByType(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _notificationService.GetNotificationsByTypeAsync(type, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search notifications with filters
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchNotifications([FromBody] NotificationSearchRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid search criteria", errors = ModelState });

                if (request.Page < 1 || request.PageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _notificationService.SearchNotificationsAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error searching notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error marking notification as read: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mark notification as unread
        /// </summary>
        [HttpPut("{id}/mark-unread")]
        public async Task<IActionResult> MarkNotificationAsUnread(int id)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsUnreadAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification marked as unread" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error marking notification as unread: {ex.Message}" });
            }
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(int userId)
        {
            try
            {
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                if (!result)
                    return NotFound(new { success = false, message = "No unread notifications found for user" });

                return Ok(new { success = true, message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error marking all notifications as read: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update notification status (read/unread)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var result = await _notificationService.UpdateNotificationStatusAsync(id, request);
                if (!result)
                    return NotFound(new { success = false, message = "Notification not found" });

                return Ok(new { success = true, message = "Notification status updated successfully" });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating notification status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete all notifications for a user
        /// </summary>
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteNotificationsByUser(int userId)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationsByUserAsync(userId);
                if (!result)
                    return NotFound(new { success = false, message = "No notifications found for user" });

                return Ok(new { success = true, message = "All user notifications deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting user notifications: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete old notifications before a specific date
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> DeleteOldNotifications([FromQuery] DateTime beforeDate)
        {
            try
            {
                var result = await _notificationService.DeleteOldNotificationsAsync(beforeDate);
                if (!result)
                    return NotFound(new { success = false, message = "No old notifications found" });

                return Ok(new { success = true, message = $"Old notifications before {beforeDate:yyyy-MM-dd} deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting old notifications: {ex.Message}" });
            }
        }
    }
}
