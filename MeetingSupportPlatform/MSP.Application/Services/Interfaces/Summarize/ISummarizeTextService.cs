using Microsoft.AspNetCore.Http;
using MSP.Application.Models.Responses.Summarize;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Summarize
{
    public interface ISummarizeTextService
    {
        Task<ApiResponse<SummarizeTextResponse>> SummarizeAsync(string text);
        Task<ApiResponse<SummarizeTextResponse>> CreateTodoListAsync(string request);
        Task<ApiResponse<SummarizeVideoTextResponse>> SummarizeVideoTextAsync(string text, IFormFile? video);
    }
}
