using Microsoft.AspNetCore.Identity;
using Moq;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using MSP.Shared.Enums;
using Xunit;
using TodoServiceImpl = MSP.Application.Services.Implementations.Todos.TodoService;

namespace MSP.Tests.Services.ToDosServicesTest
{
    public class DeleteTodoListTest
    {
        private readonly Mock<ITodoRepository> _mockTodoRepository;
        private readonly Mock<IMeetingRepository> _mockMeetingRepository;
        private readonly Mock<IProjectTaskRepository> _mockProjectTaskRepository;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ITodoService _todoService;

        public DeleteTodoListTest()
        {
            _mockTodoRepository = new Mock<ITodoRepository>();
            _mockMeetingRepository = new Mock<IMeetingRepository>();
            _mockProjectTaskRepository = new Mock<IProjectTaskRepository>();
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );

            _todoService = new TodoServiceImpl(
                _mockUserManager.Object,
                _mockTodoRepository.Object,
                _mockMeetingRepository.Object,
                _mockProjectTaskRepository.Object
            );
        }

        #region DeleteTodoAsync Tests

        [Fact]
        public async Task DeleteTodoAsync_WithValidTodoId_ReturnsSuccessResponse()
        {
            // Arrange
            var todoId = Guid.NewGuid();
            var meetingId = Guid.NewGuid();

            var existingTodo = new Todo
            {
                Id = todoId,
                MeetingId = meetingId,
                Title = "Test Todo",
                Description = "Test Description",
                Status = TodoStatus.Generated,
                IsDeleted = false,
                ReferencedTasks = new List<ProjectTask>()
            };

            _mockTodoRepository
                .Setup(x => x.GetByIdAsync(todoId))
                .ReturnsAsync(existingTodo);

            _mockTodoRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Todo>()))
                .Returns(Task.CompletedTask);

            _mockTodoRepository
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _todoService.DeleteTodoAsync(todoId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Delete todo item successfully", result.Message);

            _mockTodoRepository.Verify(x => x.GetByIdAsync(todoId), Times.Once);
            _mockTodoRepository.Verify(x => x.UpdateAsync(It.IsAny<Todo>()), Times.Once);
            _mockTodoRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTodoAsync_WithNonExistentTodoId_ReturnsErrorResponse()
        {
            // Arrange
            var todoId = Guid.NewGuid();

            _mockTodoRepository
                .Setup(x => x.GetByIdAsync(todoId))
                .ReturnsAsync((Todo?)null);

            // Act
            var result = await _todoService.DeleteTodoAsync(todoId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Todo not found", result.Message);

            _mockTodoRepository.Verify(x => x.GetByIdAsync(todoId), Times.Once);
            _mockTodoRepository.Verify(x => x.UpdateAsync(It.IsAny<Todo>()), Times.Never);
            _mockTodoRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        #endregion
    }
}
