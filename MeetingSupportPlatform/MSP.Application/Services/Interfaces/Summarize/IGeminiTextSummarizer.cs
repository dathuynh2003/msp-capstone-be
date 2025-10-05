using MSP.Application.Models.Responses.Summarize;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Summarize
{
    public interface IGeminiTextSummarizer
    {
        Task<ApiResponse<SummarizeTextResponse>> SummarizeAsync(string text);
        Task<ApiResponse<SummarizeTextResponse>> GenerateTodoListAsync(string text);
    }
}
