using MSP.Application.Models.Requests.Todo;

namespace MSP.Application.Models.Requests.Meeting
{
    public class UpdateMeetingAIDataRequest
    {
        public Guid MeetingId { get; set; }
        public string Transcription { get; set; }
        public string Summary { get; set; }
        public string? RecordUrl { get; set; }
        public List<CreateTodoRequest> Todos { get; set; } = new List<CreateTodoRequest>();
    }
}
