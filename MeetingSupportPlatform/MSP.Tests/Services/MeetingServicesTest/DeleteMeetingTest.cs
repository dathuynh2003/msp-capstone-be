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
    public class DeleteMeetingTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly IMeetingService _meetingService;
        private readonly Mock<ITodoService> _mockTodoService;

        public DeleteMeetingTest()
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

        #region DeleteMeetingAsync Tests

        [Fact]
        public async Task DeleteMeetingAsync_WithValidMeetingId_ReturnsSuccessResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                Description = "Test Description",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting deleted successfully", result.Message);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.SoftDeleteAsync(meeting), Times.Once);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMeetingAsync_WithNonExistentMeetingId_ReturnsErrorResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync((Meeting?)null);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Meeting not found", result.Message);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.SoftDeleteAsync(It.IsAny<Meeting>()), Times.Never);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteMeetingAsync_WithScheduledMeeting_DeletesSuccessfully()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Scheduled Meeting",
                StartTime = DateTime.UtcNow.AddDays(7),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting deleted successfully", result.Message);

            _mockMeetingRepository.Verify(x => x.SoftDeleteAsync(meeting), Times.Once);
        }

        [Fact]
        public async Task DeleteMeetingAsync_WithFinishedMeeting_DeletesSuccessfully()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Finished Meeting",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1),
                Status = MSP.Shared.Enums.MeetingEnum.Finished.ToString(),
                Summary = "Meeting summary",
                Transcription = "Meeting transcription",
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting deleted successfully", result.Message);
        }

        [Fact]
        public async Task DeleteMeetingAsync_WithCancelledMeeting_DeletesSuccessfully()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Cancelled Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Cancelled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting deleted successfully", result.Message);
        }

        [Fact]
        public async Task DeleteMeetingAsync_WithMeetingHavingAttendees_DeletesSuccessfully()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var attendees = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "attendee1@example.com", FullName = "Attendee 1" },
                new User { Id = Guid.NewGuid(), Email = "attendee2@example.com", FullName = "Attendee 2" },
                new User { Id = Guid.NewGuid(), Email = "attendee3@example.com", FullName = "Attendee 3" }
            };

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Team Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = attendees
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting deleted successfully", result.Message);

            _mockMeetingRepository.Verify(x => x.SoftDeleteAsync(It.Is<Meeting>(m => m.Attendees.Count == 3)), Times.Once);
        }

        [Fact]
        public async Task DeleteMeetingAsync_CallsSoftDeleteNotHardDelete()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            Meeting? capturedMeeting = null;

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedMeeting);
            Assert.Equal(meetingId, capturedMeeting.Id);

            // Verify SoftDeleteAsync is called instead of DeleteAsync or similar
            _mockMeetingRepository.Verify(x => x.SoftDeleteAsync(meeting), Times.Once);
        }

        [Fact]
        public async Task DeleteMeetingAsync_VerifiesSaveChangesIsCalled()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMeetingAsync_ReturnsNullData()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.SoftDeleteAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.DeleteMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region CancelMeetingAsync Tests

        [Fact]
        public async Task CancelMeetingAsync_WithValidMeetingId_ReturnsSuccessResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.CancelMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Meeting cancelled successfully", result.Message);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Once);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelMeetingAsync_WithNonExistentMeetingId_ReturnsErrorResponse()
        {
            // Arrange
            var meetingId = Guid.NewGuid();

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync((Meeting?)null);

            // Act
            var result = await _meetingService.CancelMeetingAsync(meetingId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Meeting not found", result.Message);

            _mockMeetingRepository.Verify(x => x.GetMeetingByIdAsync(meetingId), Times.Once);
            _mockMeetingRepository.Verify(x => x.UpdateAsync(It.IsAny<Meeting>()), Times.Never);
            _mockMeetingRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CancelMeetingAsync_SetsStatusToCancelled()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            Meeting? capturedMeeting = null;

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _meetingService.CancelMeetingAsync(meetingId);

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.Equal(MSP.Shared.Enums.MeetingEnum.Cancelled.ToString(), capturedMeeting.Status);
        }

        [Fact]
        public async Task CancelMeetingAsync_UpdatesUpdatedAtTimestamp()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var beforeTest = DateTime.UtcNow;

            Meeting? capturedMeeting = null;

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Callback<Meeting>(m => capturedMeeting = m)
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _meetingService.CancelMeetingAsync(meetingId);
            var afterTest = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedMeeting);
            Assert.True(capturedMeeting.UpdatedAt >= beforeTest && capturedMeeting.UpdatedAt <= afterTest);
        }

        [Fact]
        public async Task CancelMeetingAsync_ReturnsNullData()
        {
            // Arrange
            var meetingId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            var meeting = new Meeting
            {
                Id = meetingId,
                CreatedById = userId,
                ProjectId = projectId,
                Title = "Test Meeting",
                StartTime = DateTime.UtcNow.AddDays(1),
                Status = MSP.Shared.Enums.MeetingEnum.Scheduled.ToString(),
                Attendees = new List<User>()
            };

            _mockMeetingRepository
                .Setup(x => x.GetMeetingByIdAsync(meetingId))
                .ReturnsAsync(meeting);

            _mockMeetingRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Meeting>()))
                .Returns(Task.CompletedTask);

            _mockMeetingRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _meetingService.CancelMeetingAsync(meetingId);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion
    }
}
