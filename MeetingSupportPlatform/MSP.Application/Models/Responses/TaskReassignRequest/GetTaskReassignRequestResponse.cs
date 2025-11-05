using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.TaskReassignRequest
{
    public class GetTaskReassignRequestResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResponseMessage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public GetTaskResponse Task { get; set; }
        public GetUserResponse FromUser { get; set; }
        public GetUserResponse ToUser { get; set; }
    }
}
