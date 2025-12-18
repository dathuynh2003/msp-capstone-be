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
    public class DeleteMeetingTest
    {
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly IMeetingService _meetingService;

        public DeleteMeetingTest()
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

        #endregion
    }
}
