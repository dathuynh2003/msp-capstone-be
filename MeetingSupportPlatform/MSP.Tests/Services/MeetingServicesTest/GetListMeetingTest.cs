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

    }
}
