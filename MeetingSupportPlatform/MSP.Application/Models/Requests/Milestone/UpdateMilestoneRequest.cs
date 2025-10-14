namespace MSP.Application.Models.Requests.Milestone
{
    public class UpdateMilestoneRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
