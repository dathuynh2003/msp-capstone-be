using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.Project
{
    public class ProjectDetailResponse
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UserDto Owner { get; set; }
        public UserDto ProjectManager { get; set; }
        public List<ProjectMemberDto> Members { get; set; }
        public List<ProjectTaskDto> Tasks { get; set; }
    }
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ProjectMemberDto
    {
        public Guid? MemberId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ProjectTaskDto
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsOverdue { get; set; }
        public UserDto? Assignee { get; set; }
    }
}
