using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Meeting
{
    public class GetMeetingResponse
    {
            public Guid Id { get; set; }

            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }

            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }

            public string Status { get; set; } = string.Empty;

            public Guid CreatedById { get; set; }

            public Guid ProjectId { get; set; }
            public Guid? MilestoneId { get; set; }

            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }

            public List<AttendeeResponse> Attendees { get; set; } = new();
        }

        public class AttendeeResponse
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
        }
}
