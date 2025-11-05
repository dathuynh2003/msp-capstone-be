using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSP.Shared.Enums;

namespace MSP.Application.Models.Requests.Todo
{
    public class UpdateTodoRequest
    {
        public Guid? AssigneeId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TodoStatus Status { get; set; } = TodoStatus.UnderReview;
    }
}
