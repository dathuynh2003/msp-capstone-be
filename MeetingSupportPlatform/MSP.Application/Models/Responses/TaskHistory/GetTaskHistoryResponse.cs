using MSP.Application.Models.Responses.Auth;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Responses.TaskHistory
{
    public class GetTaskHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid? FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public GetTaskResponse Task { get; set; }
        public GetUserResponse? FromUser { get; set; }
        public GetUserResponse ToUser { get; set; }
    }
}
