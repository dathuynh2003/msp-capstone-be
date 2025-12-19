using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Notification;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using Xunit;
using MeetingServiceImpl = MSP.Application.Services.Implementations.Meeting.MeetingService;

namespace MSP.Tests.Services.MeetingServicesTest
{
    public class GetMeetingDetailTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IMeetingService _meetingService;

        public GetMeetingDetailTest()
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

        #region GetMeetingByIdAsync Tests

        [Fact]
        public async Task GetMeetingByIdAsync_WithValidId_ReturnsCompleteDetails()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
            var attendeeId1 = Guid.NewGuid();
            var attendeeId2 = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var milestone = new Milestone
            {
                Id = milestoneId,
                Name = "Sprint 1"
            };

            var attendees = new List<User>
            {
                new User { Id = attendeeId1, Email = "attendee1@example.com", FullName = "Attendee One", AvatarUrl = "avatar1.png" },
                new User { Id = attendeeId2, Email = "attendee2@example.com", FullName = "Attendee Two", AvatarUrl = "avatar2.png" }
            };

            var startTime = DateTime.UtcNow.AddDays(1);
            var endTime = startTime.AddHours(1);
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var updatedAt = DateTime.UtcNow;

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = milestoneId,
                Milestone = milestone,
                Title = "Weekly Standup Meeting",
                Description = "Weekly team sync to discuss progress",
                StartTime = startTime,
                EndTime = endTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Summary = "Team discussed sprint progress and blockers",
                Transcription = "Full meeting transcription text here...",
                RecordUrl = "https://example.com/recordings/meeting123.mp4",
                Attendees = attendees
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting retrieved successfully", result.Message);
            Assert.NotNull(result.Data);

            // Verify basic meeting info
            Assert.Equal(meetingId, result.Data.Id);
            Assert.Equal("Weekly Standup Meeting", result.Data.Title);
            Assert.Equal("Weekly team sync to discuss progress", result.Data.Description);
            Assert.Equal(startTime, result.Data.StartTime);
            Assert.Equal(endTime, result.Data.EndTime);
            Assert.Equal(MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(), result.Data.Status);

            // Verify project info
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test Project", result.Data.ProjectName);

            // Verify creator info
            Assert.Equal(userId, result.Data.CreatedById);
            Assert.Equal("creator@example.com", result.Data.CreatedByEmail);

            // Verify milestone info
            Assert.Equal(milestoneId, result.Data.MilestoneId);
            Assert.Equal("Sprint 1", result.Data.MilestoneName);

            // Verify timestamps
            Assert.Equal(createdAt, result.Data.CreatedAt);
            Assert.Equal(updatedAt, result.Data.UpdatedAt);

            // Verify extra fields
            Assert.Equal("Team discussed sprint progress and blockers", result.Data.Summary);
            Assert.Equal("Full meeting transcription text here...", result.Data.Transcription);
            Assert.Equal("https://example.com/recordings/meeting123.mp4", result.Data.RecordUrl);

            // Verify attendees
            Assert.Equal(2, result.Data.Attendees.Count);
            var attendee1 = result.Data.Attendees.First(a => a.Id == attendeeId1);
            Assert.Equal("attendee1@example.com", attendee1.Email);
            Assert.Equal("Attendee One", attendee1.FullName);
            Assert.Equal("avatar1.png", attendee1.AvatarUrl);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithNonExistentId_ReturnsErrorResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync((Meeting?)null);

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Meeting not found", result.Message);
            Assert.Null(result.Data);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithNullMilestone_ReturnsNullMilestoneData()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = null,
                Milestone = null,
                Title = "Ad-hoc Meeting",
                Description = "Quick sync",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.MilestoneId);
            Assert.Null(result.Data.MilestoneName);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithNoAttendees_ReturnsEmptyAttendeesList()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Solo Meeting",
                Description = "Personal planning",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Attendees);
            Assert.Empty(result.Data.Attendees);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithNullDescription_ReturnsEmptyDescription()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Quick Sync",
                Description = null,
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(string.Empty, result.Data.Description);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithFinishedStatus_ReturnsCorrectStatus()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = startTime.AddHours(1);

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Completed Meeting",
                Description = "This meeting has finished",
                StartTime = startTime,
                EndTime = endTime,
                Status = MSP.Shared.Enums.MeetingEnum.Finished.ToString(),
                Summary = "Meeting concluded with action items",
                Transcription = "Full transcription...",
                RecordUrl = "https://example.com/recording.mp4",
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(MSP.Shared.Enums.MeetingEnum.Finished.ToString(), result.Data.Status);
            Assert.Equal(endTime, result.Data.EndTime);
            Assert.Equal("Meeting concluded with action items", result.Data.Summary);
            Assert.Equal("Full transcription...", result.Data.Transcription);
            Assert.Equal("https://example.com/recording.mp4", result.Data.RecordUrl);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithCancelledStatus_ReturnsCorrectStatus()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Cancelled Meeting",
                Description = "This meeting was cancelled",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Cancelled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(MSP.Shared.Enums.MeetingEnum.Cancelled.ToString(), result.Data.Status);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithNullOptionalFields_ReturnsNullValues()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Basic Meeting",
                Description = null,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = null,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Summary = null,
                Transcription = null,
                RecordUrl = null,
                MilestoneId = null,
                Milestone = null,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.EndTime);
            Assert.Null(result.Data.Summary);
            Assert.Null(result.Data.Transcription);
            Assert.Null(result.Data.RecordUrl);
            Assert.Null(result.Data.MilestoneId);
            Assert.Null(result.Data.MilestoneName);
        }

        [Fact]
        public async Task GetMeetingByIdAsync_WithManyAttendees_MapsAllAttendeesCorrectly()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var attendees = new List<User>();
            for (int i = 1; i <= 10; i++)
            {
                attendees.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"attendee{i}@example.com",
                    FullName = $"Attendee {i}",
                    AvatarUrl = i % 2 == 0 ? $"avatar{i}.png" : null
                });
            }

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Large Team Meeting",
                Description = "Monthly all-hands",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = attendees
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data.Attendees.Count);

            // Verify all attendees are mapped correctly
            for (int i = 1; i <= 10; i++)
            {
                var attendee = result.Data.Attendees.FirstOrDefault(a => a.Email == $"attendee{i}@example.com");
                Assert.NotNull(attendee);
                Assert.Equal($"Attendee {i}", attendee.FullName);
                if (i % 2 == 0)
                {
                    Assert.Equal($"avatar{i}.png", attendee.AvatarUrl);
                }
                else
                {
                    Assert.Null(attendee.AvatarUrl);
                }
            }
        }

        [Fact]
        public async Task GetMeetingByIdAsync_VerifiesRepositoryCalledOnce()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "creator@example.com",
                FullName = "Creator User"
            };

            var project = new Project
            {
                Id = projectId,
                Name = "Test Project",
                CreatedById = userId,
                OwnerId = userId,
                Status = "InProgress"
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            await _meetingService.GetMeetingByIdAsync(meetingId);

            // Assert
            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        #endregion
    }
}
