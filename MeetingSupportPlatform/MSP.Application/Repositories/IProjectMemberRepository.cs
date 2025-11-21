using Microsoft.EntityFrameworkCore;
using MSP.Application.Abstracts;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IProjectMemberRepository : IGenericRepository<ProjectMember, Guid>
    {
        Task<IEnumerable<ProjectMember>> GetProjectMembersByProjectIdAsync(Guid projectId);
        Task<List<ProjectMember>> GetActiveMembershipsByMemberIdAsync(Guid memberId);
        Task<List<ProjectMember>> GetActiveMembershipsByMemberAndProjectsAsync(Guid memberId, List<Guid> projectIds);
    }
}
