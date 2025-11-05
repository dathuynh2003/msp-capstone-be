using MSP.Domain.Entities;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Todo
{
    public class GetTodoResponse
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public Guid? UserId { get; set; }
        public AssigneeResponse? Assignee  { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public TodoStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            TodoStatus.Generated => "Generated",
            TodoStatus.Deleted => "Deleted",
            TodoStatus.UnderReview => "UnderReview",
            TodoStatus.ConvertedToTask => "ConvertedToTask",
            _ => "Không xác định"
        };
    }


}
