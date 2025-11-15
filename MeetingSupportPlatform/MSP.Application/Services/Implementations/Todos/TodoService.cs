using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.Todo;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Models.Responses.ProjectTask;
using MSP.Application.Models.Responses.Todo;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Services.Implementations.Todos
{
    public class TodoService : ITodoService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITodoRepository _todoRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IMeetingRepository _meetingRepository;

        public TodoService(UserManager<User> userManager, ITodoRepository todoRepository, IMeetingRepository meetingRepository, IProjectTaskRepository projectTaskRepository)
        {
            _userManager = userManager;
            _todoRepository = todoRepository;
            _meetingRepository = meetingRepository;
            _projectTaskRepository = projectTaskRepository;
        }

        public async Task<ApiResponse<List<GetTaskResponse>>> ConvertTodosToTasksAsync(List<Guid> todoIds)
        {
            var tasks = new List<Domain.Entities.ProjectTask>();
            var todos = new List<Todo>();
            var responseTasks = new List<GetTaskResponse>();
            var failedIds = new List<Guid>();

            foreach (var todoId in todoIds)
            {
                var todo = await _todoRepository.GetByIdAsync(todoId);

                // validate đủ trường
                if (todo == null ||
                    string.IsNullOrWhiteSpace(todo.Title) ||
                    string.IsNullOrWhiteSpace(todo.Description) ||
                    todo.StartDate == null ||
                    todo.EndDate == null ||
                    todo.UserId == null ||
                    todo.Status == Shared.Enums.TodoStatus.ConvertedToTask ||
                    todo.Status == Shared.Enums.TodoStatus.Deleted)

                {
                    failedIds.Add(todoId);
                    continue;
                }

                var task = new Domain.Entities.ProjectTask
                {
                    Title = todo.Title,
                    Description = todo.Description,
                    StartDate = todo.StartDate.Value,
                    EndDate = todo.EndDate.Value,
                    UserId = todo.UserId.Value,
                    TodoId = todo.Id,
                    ProjectId = todo.Meeting.ProjectId,
                    Status = Shared.Enums.TaskEnum.NotStarted.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                todo.Status = Shared.Enums.TodoStatus.ConvertedToTask;

                todos.Add(todo);
                tasks.Add(task);

                responseTasks.Add(new GetTaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    StartDate = task.StartDate,
                    EndDate = task.EndDate,
                    Status = task.Status,
                    ProjectId = task.ProjectId,
                    UserId = task.UserId,
                    //TodoId = task.TodoId
                    // các field khác nếu có
                });
            }

            if (todos.Any())
            {
                await _todoRepository.UpdateRangeAsync(todos);
                await _todoRepository.SaveChangesAsync();
            }
            if (tasks.Any())
            {
                await _projectTaskRepository.AddRangeAsync(tasks);
                await _projectTaskRepository.SaveChangesAsync();
            }

            // Trả về response vui lòng gồm kết quả và cảnh báo các id lỗi (nếu có)
            //if (failedIds.Any())
            //{
            //    var warnMsg = $"Các To-do sau không thể convert do thiếu dữ liệu: {string.Join(", ", failedIds.Select(x => x.ToString()))}";
            //    return ApiResponse<List<GetTaskResponse>>.ErrorResponse(responseTasks, warnMsg);
            //}
            return ApiResponse<List<GetTaskResponse>>.SuccessResponse(responseTasks);
        }


        public async Task<ApiResponse<GetTodoResponse>> CreateTodoAsync(CreateTodoRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.AssigneeId.ToString());
            if (user == null)
                return ApiResponse<GetTodoResponse>.ErrorResponse(null, "User not found");

            var meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);
            if (meeting == null)
                return ApiResponse<GetTodoResponse>.ErrorResponse(null, "Meeting not found");

            // Validate referenced tasks if any
            IEnumerable<Domain.Entities.ProjectTask> referencedTasks = Enumerable.Empty<Domain.Entities.ProjectTask>();
            if (request.ReferenceTaskIds.Any())
            {
                referencedTasks = await _projectTaskRepository
                    .GetTasksByIdsAsync(request.ReferenceTaskIds);

                if (referencedTasks.Count() != request.ReferenceTaskIds.Count)
                    return ApiResponse<GetTodoResponse>.ErrorResponse(null, "Some referenced tasks not found");
            }

            var todo = new Todo
            {

                MeetingId = request.MeetingId,
                UserId = request.AssigneeId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                Status = Shared.Enums.TodoStatus.Generated
            };
            // Add referenced tasks
            if (referencedTasks.Any())
            {
                foreach (var task in referencedTasks)
                {
                    todo.ReferencedTasks.Add(task);
                }
            }

            await _todoRepository.AddAsync(todo);
            await _todoRepository.SaveChangesAsync();
            var rs = new GetTodoResponse
            {
                Id = todo.Id,
                MeetingId = todo.MeetingId,
                UserId = todo.UserId,
                Title = todo.Title,
                Description = todo.Description,
                StartDate = todo.StartDate,
                EndDate = todo.EndDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                Status = Shared.Enums.TodoStatus.Generated,
                ReferencedTasks = referencedTasks
                .Select(t => t.Id)
                .ToList()
            };
            return ApiResponse<GetTodoResponse>.SuccessResponse(rs, "Create todo successfully");
        }

        public async Task<ApiResponse<string>> DeleteTodoAsync(Guid todoId)
        {
            var todo = await _todoRepository.GetByIdAsync(todoId);
            if (todo == null)
                return ApiResponse<string>.ErrorResponse(null, "Todo not found");
            //await _todoRepository.HardDeleteAsync(todo);
            //await _todoRepository.SaveChangesAsync();
            todo.Status = Shared.Enums.TodoStatus.Deleted;
            todo.IsDeleted = true;
            await _todoRepository.UpdateAsync(todo);
            await _todoRepository.SaveChangesAsync();
            return ApiResponse<string>.SuccessResponse(null, "Delete todo item successfully");
        }

        public async Task<ApiResponse<IEnumerable<GetTodoResponse>>> GetTodoByMeetingIdAsync(Guid meetingId)
        {
            var todos = await _todoRepository.GetTodoByMeetingId(meetingId);
            var rs = todos.Select(todo => new GetTodoResponse
            {
                Id = todo.Id,
                MeetingId = todo.MeetingId,
                UserId = todo.UserId,
                Title = todo.Title,
                Description = todo.Description,
                StartDate = todo.StartDate,
                EndDate = todo.EndDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                // lấy luôn thông tin assignee
                Assignee = todo.User != null ? new AssigneeResponse
                {
                    Id = todo.User.Id,
                    FullName = todo.User.FullName,
                    Email = todo.User.Email,
                    AvatarUrl = todo.User.AvatarUrl
                } : null,
                Status = todo.Status,
                ReferencedTasks = todo.ReferencedTasks.Select(t => t.Id).ToList()
            });
            return ApiResponse<IEnumerable<GetTodoResponse>>.SuccessResponse(rs, "Get todos successfully");
        }

        public async Task<ApiResponse<GetTodoResponse>> UpdateTodoAsync(Guid todoId, UpdateTodoRequest request)
        {
            var todo = await _todoRepository.GetByIdAsync(todoId);
            if (todo == null)
                return ApiResponse<GetTodoResponse>.ErrorResponse(null, "Todo not found");


            if (!string.IsNullOrWhiteSpace(request.Title))
                todo.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.Description))
                todo.Description = request.Description;

            if (request.AssigneeId.HasValue)
                todo.UserId = request.AssigneeId.Value;

            if (request.StartDate.HasValue)
                todo.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                todo.EndDate = request.EndDate.Value;

            todo.UpdatedAt = DateTime.UtcNow;
            todo.Status = Shared.Enums.TodoStatus.UnderReview;

            await _todoRepository.UpdateAsync(todo);
            await _todoRepository.SaveChangesAsync();


            var rs = new GetTodoResponse
            {
                Id = todo.Id,
                MeetingId = todo.MeetingId,
                UserId = todo.UserId,
                Title = todo.Title,
                Description = todo.Description,
                StartDate = todo.StartDate,
                EndDate = todo.EndDate,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                Assignee = todo.User != null ? new AssigneeResponse
                {
                    Id = todo.User.Id,
                    FullName = todo.User.FullName,
                    Email = todo.User.Email,
                    AvatarUrl = todo.User.AvatarUrl
                } : null,
                Status = Shared.Enums.TodoStatus.UnderReview
            };

            return ApiResponse<GetTodoResponse>.SuccessResponse(rs, "Update todo successfully");
        }

    }
}
