using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Requests.Milestone;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Models.Responses.Milestone;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.Meeting
{
    public interface IMeetingService
    {
        Task<ApiResponse<GetMeetingResponse>> CreateMeetingAsync(CreateMeetingRequest request);
        Task<ApiResponse<GetMeetingResponse>> UpdateMeetingAsync(UpdateMeetingRequest request);
        Task<ApiResponse<string>> DeleteMeetingAsync(Guid meetingId);
        Task<ApiResponse<string>> CancelMeetingAsync(Guid meetingId);
        Task<ApiResponse<GetMeetingResponse>> GetMeetingByIdAsync(Guid meetingId);
        Task<ApiResponse<List<GetMeetingResponse>>> GetMeetingsByProjectIdAsync(Guid projectId);
    }
}
