namespace MSP.Application.Models.Requests.ProjectTask
{
    public class UpdateTaskRequest
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ActorId { get; set; }  // Person who updates/reassigns the task (PM/BO)
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid[]? MilestoneIds { get; set; }  // Chỉ lưu các ID của milestone
    }
}
