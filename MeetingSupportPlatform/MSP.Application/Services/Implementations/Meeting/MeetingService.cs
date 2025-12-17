using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Requests.Todo;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Meeting
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITodoService _todoService;
        private readonly UserManager<User> _userManager;

        public MeetingService(
            IMeetingRepository meetingRepository,
            IProjectRepository projectRepository,
            UserManager<User> userManager,
            ITodoService todoService)
        {
            _meetingRepository = meetingRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _todoService = todoService;
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

            meeting.Status = MSP.Shared.Enums.MeetingEnum.Cancelled.ToString();
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


            meeting.Title = request.Title ?? meeting.Title;
            meeting.Description = request.Description ?? meeting.Description;
            meeting.MilestoneId = request.MilestoneId ?? meeting.MilestoneId;
            meeting.StartTime = request.StartTime ?? meeting.StartTime;
            meeting.EndTime = meeting.StartTime.AddHours(1);
            meeting.UpdatedAt = DateTime.UtcNow;
            meeting.Summary = request.Summary ?? meeting.Summary;
            meeting.Transcription = request.Transcription ?? meeting.Transcription;
            meeting.RecordUrl = request.RecordUrl ?? meeting.RecordUrl;

            // Chỉ cập nhật attendees khi có danh sách mới
            if (request.AttendeeIds != null && request.AttendeeIds.Any())
            {
                var attendees = await _meetingRepository.GetAttendeesAsync(request.AttendeeIds);
                meeting.Attendees.Clear();
                foreach (var attendee in attendees)
                {
                    meeting.Attendees.Add(attendee);
                }
            }

            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            var updatedMeeting = await _meetingRepository.GetMeetingByIdAsync(meeting.Id);
            var response = MapToMeetingResponse(updatedMeeting);

            return ApiResponse<GetMeetingResponse>.SuccessResponse(response, "Meeting updated successfully");
        }


        public async Task<ApiResponse<string>> FinishMeetingAsync(Guid meetingId, FinishMeetingRequest request)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<string>.ErrorResponse(null, "Meeting not found");

            meeting.EndTime = request.EndTime;
            meeting.Status = MSP.Shared.Enums.MeetingEnum.Finished.ToString();
            meeting.UpdatedAt = DateTime.UtcNow;

            // Update RecordUrl if provided
            if (!string.IsNullOrEmpty(request.RecordUrl))
            {
                meeting.RecordUrl = request.RecordUrl;
            }

            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            return ApiResponse<string>.SuccessResponse(null, "Meeting finished successfully");
        }

        private GetMeetingResponse MapToMeetingResponse(Domain.Entities.Meeting meeting)
        {
            // Get creator's email
            string createdByEmail = string.Empty;
            if (meeting.CreatedById != Guid.Empty)
            {
                var creator = _userManager.Users.FirstOrDefault(u => u.Id == meeting.CreatedById);
                if (creator != null)
                    createdByEmail = creator.Email ?? string.Empty;
            }

            // Get project name
            string projectName = meeting.Project?.Name ?? string.Empty;

            // Get milestone name
            string? milestoneName = meeting.Milestone?.Name;

            return new GetMeetingResponse
            {
                Id = meeting.Id,
                ProjectId = meeting.ProjectId,
                ProjectName = projectName,
                CreatedById = meeting.CreatedById,
                CreatedByEmail = createdByEmail,
                MilestoneId = meeting.MilestoneId,
                MilestoneName = milestoneName,
                Title = meeting.Title,
                Description = meeting.Description ?? string.Empty,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                Status = meeting.Status,
                CreatedAt = meeting.CreatedAt,
                UpdatedAt = meeting.UpdatedAt,
                RecordUrl = meeting.RecordUrl,
                Transcription = meeting.Transcription,
                Summary = meeting.Summary,
                Attendees = meeting.Attendees?.Select(a => new AttendeeResponse
                {
                    Id = a.Id,
                    Email = a.Email,
                    AvatarUrl = a.AvatarUrl,
                    FullName = a.FullName,
                }).ToList() ?? new List<AttendeeResponse>()
            };
        }

        public async Task<ApiResponse<string>> UpdateTranscriptAsync(Guid meetingId, string transcription)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<string>.ErrorResponse("updateFailed", "Meeting not found");
            meeting.Transcription = transcription;
            meeting.UpdatedAt = DateTime.UtcNow;
            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("updateSuccess", "Meeting transcription updated successfully");
        }
        public async Task<ApiResponse<string>> UpdateSummaryAsync(Guid meetingId, string summary)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
                return ApiResponse<string>.ErrorResponse("updateFailed", "Meeting not found");
            meeting.Summary = summary;
            meeting.UpdatedAt = DateTime.UtcNow;
            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse("updateSuccess", "Meeting summary updated successfully");
        }

        public async Task<ApiResponse<List<GetMeetingResponse>>> GetMeetingsByUserIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResponse<List<GetMeetingResponse>>.ErrorResponse(null, "User not found");
            var meetings = await _meetingRepository.GetMeetingsByUserIdAsync(userId);
            var response = meetings.Select(meeting => MapToMeetingResponse(meeting)).ToList();
            return ApiResponse<List<GetMeetingResponse>>.SuccessResponse(response, "Meetings retrieved successfully");
        }

        public async Task<ApiResponse<GetMeetingResponse>> RegenerateMeetingAIDataAsync(UpdateMeetingAIDataRequest request)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);
            if (meeting == null)
                return ApiResponse<GetMeetingResponse>.ErrorResponse(null, "Meeting not found");

            // Validate transcription and summary
            if (string.IsNullOrWhiteSpace(request.Transcription))
            {
                return ApiResponse<GetMeetingResponse>.ErrorResponse(
                    null,
                    "Transcription is empty. Re-generate failed");
            }
            if (string.IsNullOrWhiteSpace(request.Summary))
            {
                return ApiResponse<GetMeetingResponse>.ErrorResponse(
                    null,
                    "Summary is empty. Re-generate failed");
            }

            // Update meeting field
            meeting.Transcription = request.Transcription;
            meeting.Summary = request.Summary;
            if (!string.IsNullOrWhiteSpace(request.RecordUrl))
            {
                meeting.RecordUrl = request.RecordUrl;
            }
            meeting.UpdatedAt = DateTime.UtcNow;
            await _meetingRepository.UpdateAsync(meeting);
            await _meetingRepository.SaveChangesAsync();

            // Soft delete all old todos
            await _todoService.SoftDeleteTodosByMeetingId(meeting.Id);
            //Create new todos
            var newTodos = request.Todos ?? new List<CreateTodoRequest>();
            foreach (var todoRequest in newTodos)
            {
                todoRequest.MeetingId = meeting.Id;
                await _todoService.CreateTodoAsync(todoRequest);
            }
            var updatedMeeting = await _meetingRepository.GetMeetingByIdAsync(meeting.Id);
            var response = MapToMeetingResponse(updatedMeeting);
            return ApiResponse<GetMeetingResponse>.SuccessResponse(response, "Meeting AI data regenerated successfully");
        }
    }
}