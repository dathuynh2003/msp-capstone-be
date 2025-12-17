namespace MSP.Application.Models.Requests.Meeting
{
    public class FinishMeetingRequest
    {
        public DateTime EndTime { get; set; }
        public string? RecordUrl { get; set; }
    }
}
