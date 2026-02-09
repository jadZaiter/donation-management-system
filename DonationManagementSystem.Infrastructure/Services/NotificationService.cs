using DonationManagementSystem.Application.Common.Interfaces;
using DonationManagementSystem.Domain.Entities;
using DonationManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _db;

        public NotificationService(IUnitOfWork uow, ApplicationDbContext db)
        {
            _uow = uow;
            _db = db;
        }

        public async Task CreateForAdminsAsync(string title, string message, string? link, NotificationType type)
        {
            // Get all admin users
            var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return;

            var adminUserIds = await _db.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            // Create notification for each admin
            foreach (var adminId in adminUserIds)
            {
                await CreateForUserAsync(adminId, title, message, link, type);
            }
        }

        public async Task CreateForUserAsync(string userId, string title, string message, string? link, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Notifications.AddAsync(notification);
            await _uow.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _uow.Notifications.Query()
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                _uow.Notifications.Update(notification);
                await _uow.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _uow.Notifications.Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _uow.Notifications.Update(notification);
            }

            await _uow.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadAsync(string userId, int take = 10)
        {
            return await _uow.Notifications.Query()
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _uow.Notifications.Query()
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<List<Notification>> GetAllAsync(string userId, int take = 50)
        {
            return await _uow.Notifications.Query()
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}