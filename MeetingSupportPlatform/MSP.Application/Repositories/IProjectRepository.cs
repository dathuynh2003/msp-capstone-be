using MSP.Application.Abstracts;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IProjectRepository : IGenericRepository<Project, Guid>
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<IEnumerable<Project>> GetProjectsByManagerIdAsync(Guid managerId);
        Task<IEnumerable<Project>> GetProjectsByBOIdAsync(Guid boId);
        Task<Project?> GetProjectByIdAsync(Guid id);
        Task<List<Guid>> GetProjectIdsByOwnerIdAsync(Guid ownerId);
    }
}
