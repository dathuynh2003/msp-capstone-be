using MSP.Domain.Entities;

namespace MSP.Application.Abstracts
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<Notification> UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}

