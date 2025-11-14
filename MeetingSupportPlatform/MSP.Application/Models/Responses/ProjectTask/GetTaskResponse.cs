using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.Milestone;
using MSP.Application.Models.Responses.TaskHistory;

namespace MSP.Application.Models.Responses.ProjectTask
{
    public class GetTaskResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public GetUserResponse? User { get; set; }
        public GetMilestoneResponse[]? Milestones { get; set; } = Array.Empty<GetMilestoneResponse>();
        public IEnumerable<GetTaskHistoryResponse>? TaskHistories { get; set; }

    }
}
