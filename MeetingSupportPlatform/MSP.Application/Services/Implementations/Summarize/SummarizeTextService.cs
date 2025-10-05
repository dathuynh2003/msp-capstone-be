using Microsoft.AspNetCore.Http;
using MSP.Application.Models.Requests.Summarize;
using MSP.Application.Models.Responses.Summarize;
using MSP.Application.Services.Interfaces.Summarize;
using MSP.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Services.Implementations.Summarize
{
    public class SummarizeTextService : ISummarizeTextService
    {
        private readonly IGeminiTextSummarizer _geminiSummarizer;

        public SummarizeTextService(IGeminiTextSummarizer geminiSummarizer)
        {
            _geminiSummarizer = geminiSummarizer;
        }

        public async Task<ApiResponse<SummarizeTextResponse>> SummarizeAsync(string request)
        {
            try
            {
                // 1. Đọc text từ request
                //var textToSummarize = await ReadTextFromRequest(request);
                var textToSummarize = request;
                if (string.IsNullOrWhiteSpace(textToSummarize))
                {
                    return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, "Input cannot be empty!");
                }

                // 2. Tóm tắt text
                return await _geminiSummarizer.SummarizeAsync(textToSummarize);
            }
            catch (ArgumentException ex)
            {
                return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, "Server error when summarizing");
            }
        }

        public async Task<ApiResponse<SummarizeTextResponse>> CreateTodoListAsync(string request)
        {
            try
            {
                // 1. Đọc text từ request
                //var textToProcess = await ReadTextFromRequest(request);
                var textToProcess = request;
                if (string.IsNullOrWhiteSpace(textToProcess))
                {
                    return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, "Input cannot be empty!");
                }

                // 2. Tạo todo list
                return await _geminiSummarizer.GenerateTodoListAsync(textToProcess);
            }
            catch (ArgumentException ex)
            {
                return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, ex.Message);
            }
            catch (Exception ex)
            {
                return ApiResponse<SummarizeTextResponse>.ErrorResponse(null, "Server error when creating todo list");
            }
        }

        private async Task<string> ReadTextFromRequest(HttpRequest request)
        {
            // Đọc raw text từ request body
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var rawText = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            // Kiểm tra Content-Type để xử lý
            var contentType = request.ContentType?.ToLower();

            if (contentType?.Contains("application/json") == true)
            {
                return ExtractTextFromJson(rawText);
            }
            else
            {
                return rawText; // Raw text mode
            }
        }

        private string ExtractTextFromJson(string jsonText)
        {
            // Thử parse JSON theo thứ tự ưu tiên

            // Cách 1: JsonDocument (nhanh nhất)
            if (TryParseWithJsonDocument(jsonText, out var text1))
                return text1;

            // Cách 2: Model deserialization
            if (TryParseWithModel(jsonText, out var text2))
                return text2;

            // Cách 3: Regex extraction (cuối cùng)
            if (TryParseWithRegex(jsonText, out var text3))
                return text3;

            return null; // Không parse được
        }

        private bool TryParseWithJsonDocument(string jsonText, out string text)
        {
            text = null;
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(jsonText);
                if (document.RootElement.TryGetProperty("text", out var textElement))
                {
                    text = textElement.GetString();
                    return !string.IsNullOrEmpty(text);
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }

        private bool TryParseWithModel(string jsonText, out string text)
        {
            text = null;
            try
            {
                var request = System.Text.Json.JsonSerializer.Deserialize<SummarizeTextRequest>(jsonText);
                text = request?.Text;
                return !string.IsNullOrEmpty(text);
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }

        private bool TryParseWithRegex(string jsonText, out string text)
        {
            text = null;
            try
            {
                var match = System.Text.RegularExpressions.Regex.Match(jsonText, @"""text""\s*:\s*""([^""]*(?:\\.[^""]*)*)""");
                if (match.Success)
                {
                    text = match.Groups[1].Value
                        .Replace("\\\"", "\"")
                        .Replace("\\n", "\n")
                        .Replace("\\r", "\r")
                        .Replace("\\t", "\t");
                    return !string.IsNullOrEmpty(text);
                }
            }
            catch
            {
                // Ignore errors
            }
            return false;
        }
    }
}
