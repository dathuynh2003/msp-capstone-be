using MSP.Application.Abstracts;
using MSP.Domain.Entities;

namespace MSP.Application.Repositories
{
    public interface IProjectTaskRepository : IGenericRepository<ProjectTask, Guid>
    {
        Task<IEnumerable<ProjectTask>> GetTasksByMilestoneIdAsync(Guid milestoneId);
        Task<ProjectTask?> GetTaskByIdAsync(Guid id);
        Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(Guid projectId);
        Task<bool> HasTaskOverlapAsync(Guid userId, Guid projectId, Guid excludeTaskId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ProjectTask>> GetTasksByIdsAsync(List<Guid> id);
        Task<IEnumerable<ProjectTask>> GetTasksByTodoIdAsync(Guid todoId);
        Task<IEnumerable<ProjectTask>> GetOverdueTasksAsync(DateTime currentTime, string overDueStatus, string completedStatus);
        Task<IEnumerable<ProjectTask>> GetTasksWithUpcomingDeadlinesAsync(DateTime startRange, DateTime endRange, string[] excludeStatuses);
    }
}
