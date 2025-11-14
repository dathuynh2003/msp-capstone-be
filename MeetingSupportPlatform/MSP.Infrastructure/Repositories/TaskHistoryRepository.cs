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
    public class TaskHistoryRepository(ApplicationDbContext context) : GenericRepository<TaskHistory, Guid>(context), ITaskHistoryRepository
    {

        public async Task<TaskHistory?> GetTaskHistoryByIdAsync(Guid id)
        {
            return await _context.TaskHistories
                .Include(th => th.Task)
                .Include(th => th.FromUser)
                .Include(th => th.ToUser)
                .FirstOrDefaultAsync(th => th.Id == id);
        }

        public async Task<IEnumerable<TaskHistory>> GetTaskHistoriesByTaskIdAsync(Guid taskId)
        {
            return await _context.TaskHistories
                .Include(th => th.Task)
                .Include(th => th.FromUser)
                .Include(th => th.ToUser)
                .Where(th => th.TaskId == taskId)
                .OrderByDescending(th => th.CreatedAt)
                .ToListAsync();

        }

    }
}
