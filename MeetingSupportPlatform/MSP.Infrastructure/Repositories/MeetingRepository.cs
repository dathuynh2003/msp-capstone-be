using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Infrastructure.Repositories
{
    public class MeetingRepository(ApplicationDbContext context) : GenericRepository<Meeting, Guid>(context), IMeetingRepository
    {

        public async Task<IEnumerable<User>> GetAttendeesAsync(IEnumerable<Guid> attendeeIds)
        {
            return await _context.Users
                .Where(u => attendeeIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<Meeting?> GetMeetingByIdAsync(Guid id)
        {
            return await _context.Meetings
                .Include(m => m.Attendees)
                .Include(m => m.CreatedBy)
                .Include(m => m.Project)
                .Include(m => m.Milestone)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Meeting>> GetMeetingByProjectIdAsync(Guid projectId)
        {
            return await _context.Meetings
                .Include(m => m.Attendees)
                .Include(m => m.CreatedBy)
                .Include(m => m.Project)
                .Include(m => m.Milestone)
                .Where(m => m.ProjectId == projectId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CancelMeetingAsync(Guid id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return false;

            meeting.Status = MSP.Shared.Enums.MeetingEnum.Cancelled.ToString();
            meeting.UpdatedAt = DateTime.UtcNow;

            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Meeting>> GetScheduledMeetingsToStartAsync(DateTime currentTime, string scheduledStatus)
        {
            return await _context.Meetings
                .Where(m =>
                    !m.IsDeleted &&
                    m.Status == scheduledStatus &&
                    m.StartTime <= currentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetOngoingMeetingsToCancelledAsync(DateTime currentTime, string ongoingStatus)
        {
            var maxLimitValue = await _context.Limitations
                .Where(l => l.LimitationType == LimitationTypeEnum.MeetingDuration.ToString())
                .MaxAsync(l => l.LimitValue);
            if (maxLimitValue == null)
            {
                maxLimitValue = 30;
            }    
            return await _context.Meetings
                .Where(m =>
                    !m.IsDeleted &&
                    m.Status == ongoingStatus &&
                    !m.EndTime.HasValue &&
                    m.StartTime.AddMinutes((double)maxLimitValue) <= currentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetUpcomingMeetingsForReminderAsync(DateTime startWindow, DateTime endWindow, string scheduledStatus)
        {
            return await _context.Meetings
                .Include(m => m.Attendees)
                .Include(m => m.CreatedBy)
                .Include(m => m.Project)
                .Where(m =>
                    !m.IsDeleted &&
                    m.Status == scheduledStatus &&
                    m.StartTime >= startWindow &&
                    m.StartTime <= endWindow)
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetMeetingsByUserIdAsync(Guid userId)
        {
            var meetings = await _context.Meetings
                .Where(m => m.Attendees.Any(a => a.Id == userId) && m.IsDeleted == false)
                .Include(m => m.Attendees)
                .Include(m => m.CreatedBy)
                .Include(m => m.Project)
                .Include(m => m.Milestone)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return meetings;
        }

        public async Task<int> CountMeetingsAsync(Guid businessOwnerId)
        {
            var query = _context.Meetings.AsQueryable();

            query = query.Where(m => m.CreatedBy.ManagedById == businessOwnerId);

            //if (startDate.HasValue)
            //    query = query.Where(m => m.StartTime.Date >= startDate.Value.Date);

            //if (endDate.HasValue)
            //    query = query.Where(m => m.StartTime.Date <= endDate.Value.Date);

            return await query.CountAsync();
        }
    }
}
