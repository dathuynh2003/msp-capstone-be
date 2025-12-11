using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Meeting;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using Xunit;
using MeetingServiceImpl = MSP.Application.Services.Implementations.Meeting.MeetingService;

namespace MSP.Tests.Services.MeetingServicesTest
{
    public class GetListMeetingTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMeetingService _meetingService;
        private readonly Mock<ITodoService> _mockTodoService;

        public GetListMeetingTest()
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

        #region GetMeetingByIdAsync Tests

        [Fact]
        public async Task GetMeetingByIdAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
            var attendeeId = Guid.NewGuid();

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
                Status = "Active"
            };

            var milestone = new Milestone
            {
                Id = milestoneId,
                Name = "Sprint 1"
            };

            var attendees = new List<User>
            {
                new User { Id = attendeeId, Email = "attendee@example.com", FullName = "Attendee User", AvatarUrl = "avatar.png" }
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                CreatedBy = user,
                ProjectId = projectId,
                Project = project,
                MilestoneId = milestoneId,
                Milestone = milestone,
                Title = "Test Meeting",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Summary = "Meeting summary",
                Transcription = "Meeting transcription",
                RecordUrl = "https://example.com/recording.mp4",
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
            Assert.Equal(meetingId, result.Data.Id);
            Assert.Equal("Test Meeting", result.Data.Title);
            Assert.Equal("Test Description", result.Data.Description);
            Assert.Equal(projectId, result.Data.ProjectId);
            Assert.Equal("Test Project", result.Data.ProjectName);
            Assert.Equal(milestoneId, result.Data.MilestoneId);
            Assert.Equal("Sprint 1", result.Data.MilestoneName);
            Assert.Equal("creator@example.com", result.Data.CreatedByEmail);
            Assert.Single(result.Data.Attendees);
            Assert.Equal(attendeeId, result.Data.Attendees.First().Id);

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
                Status = "Active"
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
                Title = "Test Meeting",
                Description = "Test Description",
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

        #endregion

        #region GetMeetingsByProjectIdAsync Tests

        [Fact]
        public async Task GetMeetingsByProjectIdAsync_WithValidProjectId_ReturnsListOfMeetings()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

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
                Status = "Active"
            };

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Meeting 1",
                    Description = "Description 1",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Meeting 2",
                    Description = "Description 2",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Meeting 3",
                    Description = "Description 3",
                    StartTime = DateTime.UtcNow.AddDays(3),
                    Status = MSP.Shared.Enums.MeetingEnum.Finished.ToString(),
                    Attendees = new List<User>()
                }
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByProjectIdAsync(projectId))
                .ReturnsAsync(meetings);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingsByProjectIdAsync(projectId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meetings retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("Meeting 1", result.Data[0].Title);
            Assert.Equal("Meeting 2", result.Data[1].Title);
            Assert.Equal("Meeting 3", result.Data[2].Title);

            _mockMeetingRepository.Verify(x => x.GetMeetingByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetMeetingsByProjectIdAsync_WithNoMeetings_ReturnsEmptyList()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByProjectIdAsync(projectId))
                .ReturnsAsync(new List<Meeting>());

            // Act
            var result = await _meetingService.GetMeetingsByProjectIdAsync(projectId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meetings retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);

            _mockMeetingRepository.Verify(x => x.GetMeetingByProjectIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetMeetingsByProjectIdAsync_WithMeetingsHavingDifferentStatuses_ReturnsAllMeetings()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

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
                Status = "Active"
            };

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Scheduled Meeting",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Finished Meeting",
                    StartTime = DateTime.UtcNow.AddDays(-1),
                    EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Finished.ToString(),
                    Attendees = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Cancelled Meeting",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    Status = MSP.Shared.Enums.MeetingEnum.Cancelled.ToString(),
                    Attendees = new List<User>()
                }
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByProjectIdAsync(projectId))
                .ReturnsAsync(meetings);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingsByProjectIdAsync(projectId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Contains(result.Data, m => m.Status == MSP.Shared.Enums.MeetingEnum.Scheduled.ToString());
            Assert.Contains(result.Data, m => m.Status == MSP.Shared.Enums.MeetingEnum.Finished.ToString());
            Assert.Contains(result.Data, m => m.Status == MSP.Shared.Enums.MeetingEnum.Cancelled.ToString());
        }

        [Fact]
        public async Task GetMeetingsByProjectIdAsync_WithMeetingsHavingAttendees_MapsAttendeesCorrectly()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
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
                Status = "Active"
            };

            var attendees = new List<User>
            {
                new User { Id = attendeeId1, Email = "attendee1@example.com", FullName = "Attendee 1", AvatarUrl = "avatar1.png" },
                new User { Id = attendeeId2, Email = "attendee2@example.com", FullName = "Attendee 2", AvatarUrl = null }
            };

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "Meeting with Attendees",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = attendees
                }
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByProjectIdAsync(projectId))
                .ReturnsAsync(meetings);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _meetingService.GetMeetingsByProjectIdAsync(projectId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(2, result.Data[0].Attendees.Count);

            var attendee1Response = result.Data[0].Attendees.First(a => a.Id == attendeeId1);
            Assert.Equal("attendee1@example.com", attendee1Response.Email);
            Assert.Equal("Attendee 1", attendee1Response.FullName);
            Assert.Equal("avatar1.png", attendee1Response.AvatarUrl);

            var attendee2Response = result.Data[0].Attendees.First(a => a.Id == attendeeId2);
            Assert.Null(attendee2Response.AvatarUrl);
        }

        #endregion

        #region GetMeetingsByUserIdAsync Tests

        [Fact]
        public async Task GetMeetingsByUserIdAsync_WithValidUserId_ReturnsListOfMeetings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
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

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "User Meeting 1",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User> { user }
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId,
                    Project = project,
                    Title = "User Meeting 2",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User> { user }
                }
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            _mockMeetingRepository
                .Setup(x => x.GetMeetingsByUserIdAsync(userId))
                .ReturnsAsync(meetings);

            // Act
            var result = await _meetingService.GetMeetingsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meetings retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("User Meeting 1", result.Data[0].Title);
            Assert.Equal("User Meeting 2", result.Data[1].Title);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockMeetingRepository.Verify(x => x.GetMeetingsByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetMeetingsByUserIdAsync_WithNonExistentUser_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _meetingService.GetMeetingsByUserIdAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
            Assert.Null(result.Data);

            _mockUserManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _mockMeetingRepository.Verify(x => x.GetMeetingsByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetMeetingsByUserIdAsync_WithNoMeetings_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User"
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockMeetingRepository
                .Setup(x => x.GetMeetingsByUserIdAsync(userId))
                .ReturnsAsync(new List<Meeting>());

            // Act
            var result = await _meetingService.GetMeetingsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meetings retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMeetingsByUserIdAsync_WithMeetingsFromMultipleProjects_ReturnsAllMeetings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId1 = Guid.NewGuid();
            var projectId2 = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                FullName = "Test User"
            };

            var project1 = new Project
            {
                Id = projectId1,
                Name = "Project 1",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var project2 = new Project
            {
                Id = projectId2,
                Name = "Project 2",
                CreatedById = userId,
                OwnerId = userId,
                Status = "Active"
            };

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId1,
                    Project = project1,
                    Title = "Project 1 Meeting",
                    StartTime = DateTime.UtcNow.AddDays(1),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User>()
                },
                new Meeting
                {
                    Id = Guid.NewGuid(),
                    CreatedById = userId,
                    CreatedBy = user,
                    ProjectId = projectId2,
                    Project = project2,
                    Title = "Project 2 Meeting",
                    StartTime = DateTime.UtcNow.AddDays(2),
                    Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                    Attendees = new List<User>()
                }
            };

            _mockUserManager
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager
                .Setup(x => x.Users)
                .Returns(new List<User> { user }.AsQueryable());

            _mockMeetingRepository
                .Setup(x => x.GetMeetingsByUserIdAsync(userId))
                .ReturnsAsync(meetings);

            // Act
            var result = await _meetingService.GetMeetingsByUserIdAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, m => m.ProjectName == "Project 1");
            Assert.Contains(result.Data, m => m.ProjectName == "Project 2");
        }

        #endregion
    }
}
