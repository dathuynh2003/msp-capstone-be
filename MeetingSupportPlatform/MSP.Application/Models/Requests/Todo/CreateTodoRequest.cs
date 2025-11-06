using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Todo
{
    public class CreateTodoRequest
    {
        public Guid MeetingId { get; set; }
        public Guid AssigneeId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<Guid> ReferenceTaskIds { get; set; } = new List<Guid>();
    }
}
