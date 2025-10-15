using MSP.Application.Models.Responses.Milestone;
using MSP.Domain.Entities;

namespace MSP.Application.Models.Requests.ProjectTask
{
    public class CreateTaskRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Guid[]? MilestoneIds { get; set; }  // Chỉ lưu các ID của milestone
    }
}
