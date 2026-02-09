using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DonationManagementSystem.Web.Pages
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public List<Notification> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }

        public NotificationsModel(INotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            Notifications = await _notificationService.GetAllAsync(userId, 100);
            UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
        }

        public async Task OnPostMarkReadAsync(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            await OnGetAsync();
        }

        public async Task OnPostMarkAllReadAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            await _notificationService.MarkAllAsReadAsync(userId);
            await OnGetAsync();
        }
    }
}