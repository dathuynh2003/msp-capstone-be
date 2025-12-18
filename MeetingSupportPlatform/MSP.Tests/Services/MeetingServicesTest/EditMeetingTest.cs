using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using Xunit;
using MeetingServiceImpl = MSP.Application.Services.Implementations.Meeting.MeetingService;

namespace MSP.Tests.Services.MeetingServicesTest
{
    public class EditMeetingTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IMeetingService _meetingService;

        public EditMeetingTest()
        {
            _mockMeetingRepository = new Mock<IMeetingRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockTodoService = new Mock<ITodoService>();
            _mockNotificationService = new Mock<INotificationService>();

            _meetingService = new MeetingServiceImpl(
                _mockMeetingRepository.Object,
                _mockProjectRepository.Object,
                _mockUserManager.Object,
                _mockTodoService.Object,
                _mockNotificationService.Object
            );
        }

        #region UpdateMeetingAsync Tests

        [Fact]
        public async Task UpdateMeetingAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
            var attendeeId1 = Guid.NewGuid();
            var attendeeId2 = Guid.NewGuid();

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                MilestoneId = null,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                MilestoneId = milestoneId,
                Title = "Updated Title",
                Description = "Updated Description",
                StartTime = DateTime.UtcNow.AddDays(2),
                AttendeeIds = new List<Guid> { attendeeId1, attendeeId2 }
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var attendees = new List<User>
            {
                new User { Id = attendeeId1, Email = "attendee1@example.com", FullName = "Attendee 1" },
                new User { Id = attendeeId2, Email = "attendee2@example.com", FullName = "Attendee 2" }
            };

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = milestoneId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime.Value,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = existingMeeting.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                Attendees = attendees
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(attendees);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting updated successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(meetingId, result.Data.Id);
            Assert.Equal(request.Title, result.Data.Title);
            Assert.Equal(request.Description, result.Data.Description);
            Assert.Equal(2, result.Data.Attendees.Count);

            _mockMeetingRepository.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Once);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithNonExistentMeeting_ReturnsErrorResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Title = "Updated Title"
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync((Meeting?)null);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Meeting not found", result.Message);
            Assert.Null(result.Data);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Title = "Updated Title"
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            Assert.Null(result.Data);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
            _mockMeetingRepository.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Never);
        }

        #endregion
    }
}
