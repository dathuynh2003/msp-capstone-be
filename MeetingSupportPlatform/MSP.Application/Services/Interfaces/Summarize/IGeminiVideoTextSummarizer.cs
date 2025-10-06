using MSP.Application.Models.Responses.Summarize;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Summarize
{
    public interface IGeminiVideoTextSummarizer
    {
        Task<ApiResponse<SummarizeVideoTextResponse>> SummarizeVideoTextAsync(string text, byte[]? videoData, string? videoFileName);
    }
}
