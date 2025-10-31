using MSP.Application.Abstracts;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IMilestoneRepository : IGenericRepository<Milestone, Guid>
    {
        Task<IEnumerable<Milestone>> GetMilestonesByProjectIdAsync(Guid projectId);
        Task<Milestone?> GetMilestoneByIdAsync(Guid id);
        Task<IEnumerable<Milestone>> GetMilestonesByIdsAsync(IEnumerable<Guid> ids);

        void Attach(Milestone milestone);
    }
}
