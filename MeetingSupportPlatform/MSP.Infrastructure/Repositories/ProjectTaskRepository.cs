using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class ProjectTaskRepository(ApplicationDbContext context) : GenericRepository<ProjectTask, Guid>(context), IProjectTaskRepository
    {
        public async Task<IEnumerable<ProjectTask>> GetTasksByMilestoneIdAsync(Guid milestoneId)
        {
            return await _context.ProjectTasks
                .Include(pt => pt.Milestones)
                .Include(pt => pt.User)
                .Where(pt => pt.Milestones.Any(m => m.Id == milestoneId) && !pt.IsDeleted)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(Guid id)
        {
            return await _context.ProjectTasks
                .Include(pt => pt.Milestones)
                .Include(pt => pt.User)
                .FirstOrDefaultAsync(pt => pt.Id == id && !pt.IsDeleted);
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId)
        {
            return await _context.ProjectTasks
                .Where(pt => pt.ProjectId == projectId && !pt.IsDeleted)
                .Include(pt => pt.Milestones)
                .Include(pt => pt.User)
                .ToListAsync();
        }

        public async Task<bool> HasTaskOverlapAsync(Guid userId, Guid projectId, Guid excludeTaskId, DateTime startDate, DateTime endDate)
        {
            return await _context.ProjectTasks.AnyAsync(t =>
                t.UserId == userId &&
                t.ProjectId == projectId &&
                t.Id != excludeTaskId && // loại trừ task hiện tại
                !(t.EndDate <= startDate || t.StartDate >= endDate)
            );
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByIdsAsync(List<Guid> id)
        {
            return await _context.ProjectTasks
                .Where(pt => id.Contains(pt.Id) && !pt.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByTodoIdAsync(Guid todoId)
        {
            return await _context.ProjectTasks
                .Where(pt => pt.ReferencingTodos.Any(t => t.Id == todoId) && !pt.IsDeleted)
                .Include(pt => pt.User)
                .Include(pt => pt.Milestones)
                 .ToListAsync();
        }
        public async Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(DateTime currentTime, string overDueStatus, string completedStatus)
        {
            return await _context.ProjectTasks
                .Where(task =>
                    !task.IsDeleted &&
                    task.EndDate.HasValue &&
                    task.EndDate.Value < currentTime &&
                    task.Status != overDueStatus &&
                    task.Status != completedStatus)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksWithUpcomingDeadlinesAsync(DateTime startRange, DateTime endRange, string[] excludeStatuses)
        {
            return await _context.ProjectTasks
                .Where(task =>
                    !task.IsDeleted &&
                    task.EndDate.HasValue &&
                    task.EndDate.Value.Date >= startRange.Date &&
                    task.EndDate.Value.Date <= endRange.Date &&
                    task.UserId.HasValue &&
                    !excludeStatuses.Contains(task.Status))
                .ToListAsync();
        }
    }
}
