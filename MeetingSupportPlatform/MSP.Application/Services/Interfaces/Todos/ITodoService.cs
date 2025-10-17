using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Requests.Todo;
using MSP.Application.Models.Responses.Meeting;
using MSP.Application.Models.Responses.Todo;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Interfaces.Todos
{
    public interface ITodoService
    {
        Task<ApiResponse<GetTodoResponse>> CreateTodoAsync(CreateTodoRequest request);
        Task<ApiResponse<GetTodoResponse>> UpdateTodoAsync(Guid todoId,UpdateTodoRequest request);
        Task<ApiResponse<IEnumerable<GetTodoResponse>>> GetTodoByMeetingIdAsync(Guid meetingId);
        Task<ApiResponse<string>> DeleteTodoAsync(Guid todoId);

    }
}
