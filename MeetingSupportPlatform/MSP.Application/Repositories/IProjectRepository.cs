using MSP.Application.Abstracts;
using MSP.Application.Models.Responses.Project;
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
        Task<IEnumerable<Project>> GetNotStartedProjectsProjectsToStartAsync(DateTime currentTime, string notStartedStatus);
        Task<IEnumerable<Project>> GetProjectsNearingDeadlineAsync(DateTime currentTime, DateTime deadlineThreshold, string inProgressStatus);
        Task<IEnumerable<Project>> GetOverdueInProgressProjectsAsync(DateTime currentTime, string inProgressStatus);
        Task<int> CountProjectsAsync(Guid userId);
        Task<Project> GetProjectDetailWithPMAsync(Guid projectId, Guid userId);
        Task<Project> GetProjectDetailWithMemberAsync(Guid projectId, Guid userId);
        Task<bool> IsActiveUserInProject(Guid projectId, Guid userId);


    }
}
