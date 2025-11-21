using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class ProjectRepository(ApplicationDbContext context) : GenericRepository<Project, Guid>(context), IProjectRepository
    {
        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.Owner)
                .Include(p => p.CreatedBy)
                .Include(p => p.Milestones)
                .Include(p => p.ProjectMembers)
                .Include(p => p.Meetings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Project>> GetProjectsByManagerIdAsync(Guid managerId)
        {
            return await _context.Projects
                .Where(p => p.CreatedById == managerId && !p.IsDeleted)
                .Include(p => p.Owner)
                .Include(p => p.CreatedBy)
                .Include(p => p.Milestones)
                .Include(p => p.ProjectMembers)
                .Include(p => p.Meetings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Project>> GetProjectsByBOIdAsync(Guid boId)
        {
            return await _context.Projects
                .Where(p => p.OwnerId == boId && !p.IsDeleted)
                .Include(p => p.Owner)
                .Include(p => p.CreatedBy)
                .Include(p => p.Milestones)
                .Include(p => p.ProjectMembers)
                .Include(p => p.Meetings)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            return await _context.Projects
                .Include(p => p.Owner)
                .Include(p => p.CreatedBy)
                .Include(p => p.Milestones)
                .Include(p => p.ProjectMembers)
                .Include(p => p.Meetings)
                .Include(p => p.ProjectTasks)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<List<Guid>> GetProjectIdsByOwnerIdAsync(Guid ownerId)
        {
            return await _context.Projects
                .Where(p => p.OwnerId == ownerId)
                .Select(p => p.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetNotStartedProjectsProjectsToStartAsync(DateTime currentTime, string notStartedStatus)
        {
            return await _context.Projects
                .Where(p =>
                    !p.IsDeleted &&
                    p.Status == notStartedStatus &&
                    p.StartDate.HasValue &&
                    p.StartDate.Value <= currentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsNearingDeadlineAsync(DateTime currentTime, DateTime deadlineThreshold, string inProgressStatus)
        {
            return await _context.Projects
                .Where(p =>
                    !p.IsDeleted &&
                    p.Status == inProgressStatus &&
                    p.EndDate.HasValue &&
                    p.EndDate.Value >= currentTime &&
                    p.EndDate.Value <= deadlineThreshold)
                .Include(p => p.Owner)
                .Include(p => p.CreatedBy)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetOverdueInProgressProjectsAsync(DateTime currentTime, string inProgressStatus)
        {
            return await _context.Projects
                .Where(p =>
                    !p.IsDeleted &&
                    p.Status == inProgressStatus &&
                    p.EndDate.HasValue &&
                    p.EndDate.Value.Date < currentTime.Date)
                .Include(p => p.Owner)
                .ToListAsync();
        }

        public async Task<int> CountProjectsAsync(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Projects.AsQueryable();
            query = query.Where(p => !p.IsDeleted && p.OwnerId == userId);
            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= endDate.Value.Date);
            }
            return await query.CountAsync();
        }
    }
}
