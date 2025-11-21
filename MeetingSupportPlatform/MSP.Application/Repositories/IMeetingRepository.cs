using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface IMeetingRepository : IGenericRepository<Meeting, Guid>
    {
        Task<Meeting?> GetMeetingByIdAsync(Guid id);
        Task<IEnumerable<Meeting>> GetMeetingByProjectIdAsync(Guid projectId);
        Task<bool> CancelMeetingAsync(Guid id);
        Task<IEnumerable<User>> GetAttendeesAsync(IEnumerable<Guid> attendeeIds);
        Task<IEnumerable<Meeting>> GetScheduledMeetingsToStartAsync(DateTime currentTime, string scheduledStatus);
        Task<IEnumerable<Meeting>> GetOngoingMeetingsToFinishAsync(DateTime currentTime, string ongoingStatus);
        Task<IEnumerable<Meeting>> GetMeetingsByUserIdAsync(Guid userId);

        Task<int> CountMeetingsAsync(Guid businessOwnerId, DateTime? startDate, DateTime? endDate);
    }
}
