using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class TaskHistoryRepository(ApplicationDbContext context) : GenericRepository<TaskHistory, Guid>(context), ITaskHistoryRepository
    {

        public async Task<TaskHistory?> GetTaskHistoryByIdAsync(Guid id)
        {
            return await _context.TaskHistories
                .Include(th => th.Task)
                .Include(th => th.FromUser)
                .Include(th => th.ToUser)
                .Include(th => th.ChangedBy)
                .FirstOrDefaultAsync(th => th.Id == id);
        }

        public async Task<IEnumerable<TaskHistory>> GetTaskHistoriesByTaskIdAsync(Guid taskId)
        {
            return await _context.TaskHistories
                .Include(th => th.Task)
                .Include(th => th.FromUser)
                .Include(th => th.ToUser)
                .Include(th => th.ChangedBy)
                .Where(th => th.TaskId == taskId)
                .OrderByDescending(th => th.CreatedAt)
                .ToListAsync();

        }
    }
}
