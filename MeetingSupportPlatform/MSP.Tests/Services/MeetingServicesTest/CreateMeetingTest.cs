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
    public class CreateMeetingTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IMeetingService _meetingService;

        public CreateMeetingTest()
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

        #region CreateMeetingAsync Tests

        [Fact]
        public async Task CreateMeetingAsync_WithValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
            var attendeeId1 = Guid.NewGuid();
            var attendeeId2 = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                MilestoneId = milestoneId,
                Title = "Test Meeting",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
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

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = milestoneId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = attendees
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(attendees);

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(meetingId, result.Data.Id);
            Assert.Equal(request.Title, result.Data.Title);
            Assert.Equal(request.Description, result.Data.Description);
            Assert.Equal(2, result.Data.Attendees.Count);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
            _mockMeetingRepository.Verify(x => x.GetAttendeesAsync(request.AttendeeIds), Times.Once);
            _mockMeetingRepository.Verify(x => x.AddAsync(It.IsAny<Meeting>()), Times.Once);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateMeetingAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = Guid.NewGuid(),
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid>()
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockMeetingRepository.Verify(x => x.AddAsync(It.IsAny<Meeting>()), Times.Never);
        }

        [Fact]
        public async Task CreateMeetingAsync_WithNonExistentProject_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = Guid.NewGuid(),
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid>()
            };

            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                FullName = "Test User"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Project not found", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockProjectRepository.Verify(x => x.GetByIdAsync(projectId), Times.Once);
            _mockMeetingRepository.Verify(x => x.AddAsync(It.IsAny<Meeting>()), Times.Never);
        }

        [Fact]
        public async Task CreateMeetingAsync_WithNoAttendees_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                MilestoneId = null,
                Title = "Meeting Without Attendees",
                Description = "Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid>()
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

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = null,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(new List<User>());

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting created successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(meetingId, result.Data.Id);
            Assert.Empty(result.Data.Attendees);
        }

        [Fact]
        public async Task CreateMeetingAsync_WithNullMilestoneId_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var attendeeId = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                MilestoneId = null,
                Title = "Meeting Without Milestone",
                Description = "Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid> { attendeeId }
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
                new User { Id = attendeeId, Email = "attendee@example.com", FullName = "Attendee" }
            };

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = null,
                Milestone = null,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = attendees
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(attendees);

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.MilestoneId);
            Assert.Null(result.Data.MilestoneName);
        }

        [Fact]
        public async Task CreateMeetingAsync_SetsCorrectMeetingStatus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                Description = "Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid>()
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

            Meeting? capturedMeeting = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(new List<User>());

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.Equal(MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(), capturedMeeting.Status);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateMeetingAsync_SetsCorrectTimestamps()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var beforeTest = DateTime.UtcNow;

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                Description = "Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = new List<Guid>()
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

            Meeting? capturedMeeting = null;

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(request.AttendeeIds))
                .ReturnsAsync(new List<User>());

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            await _meetingService.CreateMeetingAsync(request);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.True(capturedMeeting.CreatedAt >= beforeTest && capturedMeeting.CreatedAt <= afterTest);
            Assert.True(capturedMeeting.UpdatedAt >= beforeTest && capturedMeeting.UpdatedAt <= afterTest);
        }

        [Fact]
        public async Task CreateMeetingAsync_WithMultipleAttendees_MapsAttendeesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();
            var attendeeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var request = new CreateMeetingRequest
            {
                MeetingId = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Team Meeting",
                Description = "Weekly sync",
                StartTime = DateTime.UtcNow.AddDays(1),
                AttendeeIds = attendeeIds
            };

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

            var attendees = new List<User>
            {
                new User { Id = attendeeIds[0], Email = "attendee1@example.com", FullName = "Attendee One", AvatarUrl = "url1" },
                new User { Id = attendeeIds[1], Email = "attendee2@example.com", FullName = "Attendee Two", AvatarUrl = "url2" },
                new User { Id = attendeeIds[2], Email = "attendee3@example.com", FullName = "Attendee Three", AvatarUrl = null }
            };

            var createdMeeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Attendees = attendees
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockProjectRepository
                .Setup(x => x.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _mockMeetingRepository
                .Setup(x => x.GetAttendeesAsync(attendeeIds))
                .ReturnsAsync(attendees);

            _mockMeetingRepository
                .Setup(x => x.AddAsync(It.IsAny<Meeting>()))
                .ReturnsAsync((Meeting m) => m);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(createdMeeting);

            // Act
            var result = await _meetingService.CreateMeetingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Attendees.Count);

            var attendeeResponse1 = result.Data.Attendees.First(a => a.Id == attendeeIds[0]);
            Assert.Equal("attendee1@example.com", attendeeResponse1.Email);
            Assert.Equal("Attendee One", attendeeResponse1.FullName);
            Assert.Equal("url1", attendeeResponse1.AvatarUrl);

            var attendeeResponse3 = result.Data.Attendees.First(a => a.Id == attendeeIds[2]);
            Assert.Null(attendeeResponse3.AvatarUrl);
        }

        #endregion
    }
}
