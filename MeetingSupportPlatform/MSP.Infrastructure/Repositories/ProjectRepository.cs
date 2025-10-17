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
    }
}
