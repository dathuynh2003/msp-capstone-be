using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Meeting
{
    public class UpdateMeetingRequest
    {
        public Guid MeetingId { get; set; }
        public Guid CreatedById { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? MilestoneId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public List<Guid> AttendeeIds { get; set; } = new List<Guid>();
    }
}
