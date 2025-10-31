using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class MilestoneRepository(ApplicationDbContext context) : GenericRepository<Milestone, Guid>(context), IMilestoneRepository
    {
        public async Task<IEnumerable<Milestone>> GetMilestonesByProjectIdAsync(Guid projectId)
        {
            return await _context.Milestones
                .AsNoTracking()
                .Where(m => m.ProjectId == projectId && !m.IsDeleted)
                .Include(m => m.ProjectTasks)
                .Include(m => m.User)
                .OrderBy(m => m.DueDate)
                .ToListAsync();
        }

        public async Task<Milestone?> GetMilestoneByIdAsync(Guid id)
        {
            return await _context.Milestones
                .AsNoTracking()
                .Include(m => m.ProjectTasks)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Milestones
                //.AsNoTracking()
                .Where(m => ids.Contains(m.Id) && !m.IsDeleted)
                .Include(m => m.ProjectTasks)
                .Include(m => m.User)
                .ToListAsync();
        }

        public void Attach(Milestone milestone)
        {
            if (milestone == null)
            {
                throw new ArgumentNullException(nameof(milestone));
            }

            var tracked = _context.ChangeTracker.Entries<Milestone>()
                           .FirstOrDefault(e => e.Entity.Id == milestone.Id);

            if (tracked == null)
            {
                _context.Attach(milestone);
            }
        }

    }
}
