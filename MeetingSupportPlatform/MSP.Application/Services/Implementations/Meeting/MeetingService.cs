using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<User> _userManager;

        public MeetingService(
            IMeetingRepository meetingRepository,
            IProjectRepository projectRepository,
            UserManager<User> userManager)
        {
            _meetingRepository = meetingRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        public async Task<ApiResponse<GetMeetingResponse>> CreateMeetingAsync(CreateMeetingRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.CreatedById.ToString());
            if (user == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "User not found");

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "Project not found");

            // Lấy danh sách attendees
            var attendees = await _meetingRepository.GetAttendeesAsync(request.AttendeeIds);

            var meeting = new Domain.Entities.Meeting
            {
                Id = request.MeetingId,
                CreatedById = request.CreatedById,
                ProjectId = request.ProjectId,
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.StartTime.AddHours(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = attendees.ToList()
            };

            await _meetingRepository.AddAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            // Lấy lại meeting với đầy đủ thông tin
            var createdMeeting = await _meetingRepository.GetMeetingByIdAsync(meeting.Id);

            var response = MapToMeetingResponse(createdMeeting);
            return ApiResponse<GetMeetingResponse>.SuccessResponse(response, "Meeting created successfully");
        }

        public async Task<ApiResponse<string>> DeleteMeetingAsync(Guid meetingId)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<string>.ErrorResponse(null, "Meeting not found");

            await _meetingRepository.SoftDeleteAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(null, "Meeting deleted successfully");
        }

        public async Task<ApiResponse<GetMeetingResponse>> GetMeetingByIdAsync(Guid meetingId)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "Meeting not found");

            var response = MapToMeetingResponse(meeting);
            return ApiResponse<GetMeetingResponse>.SuccessResponse(response, "Meeting retrieved successfully");
        }

        public async Task<ApiResponse<List<GetMeetingResponse>>> GetMeetingsByProjectIdAsync(Guid projectId)
        {
            var meetings = await _meetingRepository.GetMeetingByProjectIdAsync(projectId);
            var response = meetings.Select(meeting => MapToMeetingResponse(meeting)).ToList();

            return ApiResponse<List<GetMeetingResponse>>.SuccessResponse(response, "Meetings retrieved successfully");
        }

        public async Task<ApiResponse<string>> CancelMeetingAsync(Guid meetingId)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<string>.ErrorResponse(null, "Meeting not found");

            meeting.Status = MSP.Shared.Enums.MeetingEnum.Cancel.ToString();
            meeting.UpdatedAt = DateTime.UtcNow;

            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(null, "Meeting cancelled successfully");
        }

        public async Task<ApiResponse<GetMeetingResponse>> UpdateMeetingAsync(UpdateMeetingRequest request)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);
            if (meeting == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "Meeting not found");

            var project = await _projectRepository.GetByIdAsync(meeting.ProjectId);
            if (project == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "Project not found");

            // Lấy danh sách attendees mới
            var attendees = await _meetingRepository.GetAttendeesAsync(request.AttendeeIds);

            // Cập nhật thông tin meeting
            meeting.Title = request.Title;
            meeting.Description = request.Description;
            meeting.MilestoneId = request.MilestoneId;
            meeting.StartTime = request.StartTime;
            meeting.UpdatedAt = DateTime.UtcNow;
            meeting.EndTime = request.StartTime.AddHours(1);

            // Cập nhật attendees - clear existing và add new
            meeting.Attendees.Clear();
            foreach (var attendee in attendees)
            {
                meeting.Attendees.Add(attendee);
            }

            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            // Lấy lại meeting với đầy đủ thông tin
            var updatedMeeting = await _meetingRepository.GetMeetingByIdAsync(meeting.Id);
            var response = MapToMeetingResponse(updatedMeeting);

            return ApiResponse<GetMeetingResponse>.SuccessResponse(response, "Meeting updated successfully");
        }

        private GetMeetingResponse MapToMeetingResponse(Domain.Entities.Meeting meeting)
        {
            return new GetMeetingResponse
            {
                Id = meeting.Id,
                ProjectId = meeting.ProjectId,
                CreatedById = meeting.CreatedById,
                MilestoneId = meeting.MilestoneId,
                Title = meeting.Title,
                Description = meeting.Description ?? string.Empty,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Status = meeting.Status,
                CreatedAt = meeting.CreatedAt,
                UpdatedAt = meeting.UpdatedAt,
                Attendees = meeting.Attendees?.Select(a => new AttendeeResponse
                {
                    Id = a.Id,
                    Email = a.Email,
                    AvatarUrl = a.AvatarUrl
                }).ToList() ?? new List<AttendeeResponse>()
            };
        }
    }
}