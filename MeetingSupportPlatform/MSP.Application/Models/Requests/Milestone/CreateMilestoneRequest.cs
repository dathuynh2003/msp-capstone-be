namespace MSP.Application.Models.Requests.Milestone
{
    public class CreateMilestoneRequest
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
