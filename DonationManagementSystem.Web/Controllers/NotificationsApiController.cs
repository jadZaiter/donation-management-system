using DonationManagementSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DonationManagementSystem.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(INotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "User not authenticated" });

                var notifications = await _notificationService.GetUnreadAsync(userId, 10);
                var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(new
                {
                    unreadCount = unreadCount,
                    notifications = notifications.Select(n => new
                    {
                        id = n.Id,
                        title = n.Title,
                        message = n.Message,
                        link = FixNotificationLink(n.Link), // ? FIX LINKS HERE
                        isRead = n.IsRead,
                        createdAt = n.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetUnread: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MarkRead: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "User not authenticated" });

                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MarkAllRead: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper method to fix notification links
        private string? FixNotificationLink(string? link)
        {
            if (string.IsNullOrEmpty(link))
                return null;

            // No need to convert - links are already correct format
            return link;
        }
    }
}