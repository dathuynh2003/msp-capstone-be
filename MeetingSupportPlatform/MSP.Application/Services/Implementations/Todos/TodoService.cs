using Microsoft.AspNetCore.Identity;
using MSP.Application.Models.Requests.Todo;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Models.Responses.Todo;
using MSP.Application.Repositories;
using MSP.Application.Services.Interfaces.Todos;
using MSP.Domain.Entities;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Implementations.Todos
{
    public class TodoService : ITodoService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITodoRepository _todoRepository;
        private readonly IMeetingRepository _meetingRepository;

        public TodoService(UserManager<User> userManager , ITodoRepository todoRepository, IMeetingRepository meetingRepository)
        {
            _userManager = userManager;
            _todoRepository = todoRepository;
            _meetingRepository = meetingRepository;
        }
        public async Task<ApiResponse<GetTodoResponse>> CreateTodoAsync(CreateTodoRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.AssigneeId.ToString());
            if (user == null)
                return ApiResponse<GetTodoResponse>.ErrorResponse(null, "User not found");
            var meeting = await _meetingRepository.GetMeetingByIdAsync(request.MeetingId);
            if (meeting == null)
                return ApiResponse<GetTodoResponse>.ErrorResponse(null, "Meeting not found");
            var todo = new Todo
            {

                MeetingId = request.MeetingId,
                UserId = request.AssigneeId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
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
                UpdatedAt = todo.UpdatedAt
            };
            return ApiResponse<GetTodoResponse>.SuccessResponse(rs, "Create todo successfully");
        }

        public async Task<ApiResponse<string>> DeleteTodoAsync(Guid todoId)
        {
            var todo = await _todoRepository.GetByIdAsync(todoId);
            if (todo == null)
                return ApiResponse<string>.ErrorResponse(null, "Todo not found");
            await _todoRepository.HardDeleteAsync(todo);
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
                } : null
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
                } : null
            };

            return ApiResponse<GetTodoResponse>.SuccessResponse(rs, "Update todo successfully");
        }

    }
}
