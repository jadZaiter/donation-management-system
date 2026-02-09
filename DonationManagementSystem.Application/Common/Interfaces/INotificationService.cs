using DonationManagementSystem.Domain.Entities;

namespace DonationManagementSystem.Application.Common.Interfaces
{
    public interface INotificationService
    {
        // Create notifications
        Task CreateForAdminsAsync(string title, string message, string? link, NotificationType type);
        Task CreateForUserAsync(string userId, string title, string message, string? link, NotificationType type);
        
        // Read notifications
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        
        // Get notifications
        Task<List<Notification>> GetUnreadAsync(string userId, int take = 10);
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetAllAsync(string userId, int take = 50);
    }
}