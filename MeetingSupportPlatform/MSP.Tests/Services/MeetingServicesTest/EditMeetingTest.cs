using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
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
        private readonly IMeetingService _meetingService;
        private readonly Mock<ITodoService> _mockTodoService;

        public EditMeetingTest()
        {
            _mockMeetingRepository = new Mock<IMeetingRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockTodoService = new Mock<ITodoService>();

            _meetingService = new MeetingServiceImpl(
                _mockMeetingRepository.Object,
                _mockProjectRepository.Object,
                _mockUserManager.Object,
                _mockTodoService.Object
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
                Status = "Active"
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

        [Fact]
        public async Task UpdateMeetingAsync_WithPartialUpdate_OnlyUpdatesProvidedFields()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();

            var originalTitle = "Original Title";
            var originalDescription = "Original Description";
            var originalStartTime = DateTime.UtcNow.AddDays(1);

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = originalTitle,
                Description = originalDescription,
                StartTime = originalStartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Title = "Updated Title Only"
                // Description and StartTime are null - should keep original values
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            Meeting? capturedMeeting = null;

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = originalDescription,
                StartTime = originalStartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMeeting);
            Assert.Equal("Updated Title Only", capturedMeeting.Title);
            Assert.Equal(originalDescription, capturedMeeting.Description);
            Assert.Equal(originalStartTime, capturedMeeting.StartTime);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithNoAttendees_KeepsExistingAttendees()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var existingAttendeeId = Guid.NewGuid();

            var existingAttendees = new List<User>
            {
                new User { Id = existingAttendeeId, Email = "existing@example.com", FullName = "Existing Attendee" }
            };

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = existingAttendees
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Title = "Updated Title",
                AttendeeIds = new List<Guid>() // Empty list - should not update attendees
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = existingMeeting.Description,
                StartTime = existingMeeting.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = existingAttendees
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.Attendees);
            Assert.Equal(existingAttendeeId, result.Data.Attendees.First().Id);

            // Verify GetAttendeesAsync was never called because AttendeeIds is empty
            _mockMeetingRepository.Verify(x => x.GetAttendeesAsync(It.IsAny<List<Guid>>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithNewAttendees_ReplacesExistingAttendees()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var existingAttendeeId = Guid.NewGuid();
            var newAttendeeId1 = Guid.NewGuid();
            var newAttendeeId2 = Guid.NewGuid();

            var existingAttendees = new List<User>
            {
                new User { Id = existingAttendeeId, Email = "existing@example.com", FullName = "Existing Attendee" }
            };

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = existingAttendees
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Title = "Updated Title",
                AttendeeIds = new List<Guid> { newAttendeeId1, newAttendeeId2 }
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            var newAttendees = new List<User>
            {
                new User { Id = newAttendeeId1, Email = "new1@example.com", FullName = "New Attendee 1" },
                new User { Id = newAttendeeId2, Email = "new2@example.com", FullName = "New Attendee 2" }
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(newAttendees);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = existingMeeting.Description,
                StartTime = existingMeeting.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = newAttendees
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Attendees.Count);
            Assert.DoesNotContain(result.Data.Attendees, a => a.Id == existingAttendeeId);
            Assert.Contains(result.Data.Attendees, a => a.Id == newAttendeeId1);
            Assert.Contains(result.Data.Attendees, a => a.Id == newAttendeeId2);

            _mockMeetingRepository.Verify(x => x.GetAttendeesAsync(request.AttendeeIds), Times.Once);
        }

        [Fact]
        public async Task UpdateMeetingAsync_SetsUpdatedAtTimestamp()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var beforeTest = DateTime.UtcNow;

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
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
                Title = "Updated Title"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            Meeting? capturedMeeting = null;

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = existingMeeting.Description,
                StartTime = existingMeeting.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            await _meetingService.UpdateMeetingAsync(request);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.True(capturedMeeting.UpdatedAt >= beforeTest && capturedMeeting.UpdatedAt <= afterTest);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithSummaryAndTranscription_UpdatesCorrectly()
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
                Summary = null,
                Transcription = null,
                RecordUrl = null,
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                Summary = "Meeting summary content",
                Transcription = "Full transcription text",
                RecordUrl = "https://example.com/recording.mp4"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            Meeting? capturedMeeting = null;

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = existingMeeting.Title,
                Description = existingMeeting.Description,
                StartTime = existingMeeting.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Summary = request.Summary,
                Transcription = request.Transcription,
                RecordUrl = request.RecordUrl,
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMeeting);
            Assert.Equal(request.Summary, capturedMeeting.Summary);
            Assert.Equal(request.Transcription, capturedMeeting.Transcription);
            Assert.Equal(request.RecordUrl, capturedMeeting.RecordUrl);
        }

        [Fact]
        public async Task UpdateMeetingAsync_WithMilestoneIdChange_UpdatesMilestoneId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var originalMilestoneId = Guid.NewGuid();
            var newMilestoneId = Guid.NewGuid();

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                MilestoneId = originalMilestoneId,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                MilestoneId = newMilestoneId
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            var newMilestone = new Milestone
            {
                Id = newMilestoneId,
                Name = "New Milestone"
            };

            Meeting? capturedMeeting = null;

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = newMilestoneId,
                Milestone = newMilestone,
                Title = existingMeeting.Title,
                Description = existingMeeting.Description,
                StartTime = existingMeeting.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            var result = await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMeeting);
            Assert.Equal(newMilestoneId, capturedMeeting.MilestoneId);
            Assert.NotNull(result.Data);
            Assert.Equal(newMilestoneId, result.Data.MilestoneId);
            Assert.Equal("New Milestone", result.Data.MilestoneName);
        }

        [Fact]
        public async Task UpdateMeetingAsync_SetsEndTimeToOneHourAfterStartTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var newStartTime = DateTime.UtcNow.AddDays(3);

            var existingMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Original Title",
                Description = "Original Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = null,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            var request = new UpdateMeetingRequest
            {
                MeetingId = meetingId,
                StartTime = newStartTime
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            Meeting? capturedMeeting = null;

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var updatedMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = existingMeeting.Title,
                Description = existingMeeting.Description,
                StartTime = newStartTime,
                EndTime = newStartTime.AddHours(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .SetupSequence(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(existingMeeting)
                .ReturnsAsync(updatedMeeting);

            // Act
            await _meetingService.UpdateMeetingAsync(request);

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.Equal(newStartTime, capturedMeeting.StartTime);
            Assert.Equal(newStartTime.AddHours(1), capturedMeeting.EndTime);
        }

        #endregion
    }
}
