using Mscc.GenerativeAI;
using MSP.Shared.Common;
using MSP.Application.Models.Responses.Summarize;
using MSP.Application.Services.Interfaces.Summarize;

namespace MSP.Infrastructure.Processors
{
    public class GeminiVideoTextSummarizer : IGeminiVideoTextSummarizer
    {
        private readonly GenerativeModel _model;

        public GeminiVideoTextSummarizer(GenerativeModel model)
        {
            _model = model;
        }

        public async Task<ApiResponse<SummarizeVideoTextResponse>> SummarizeVideoTextAsync(string text, byte[]? videoData, string? videoFileName)
        {
            try
            {
                // 1. Kiểm tra đầu vào
                if (string.IsNullOrWhiteSpace(text))
                    throw new Exception("Text input cannot be null or empty");

                // 2. Làm sạch text đầu vào
                var cleanedText = CleanInputText(text);

                // 3. Tạo prompt cho video + text transcript generation
                var prompt = @$"You are an expert Vietnamese transcriber and content analyst. Your task is to create an accurate Vietnamese transcript based on the provided English transcript and video content (if available).

                ## TRANSCRIPT GENERATION INSTRUCTIONS:

                ### STEP 1: CONTENT ANALYSIS
                Analyze the provided English transcript to understand:
                - **Meeting Type**: Business meeting, presentation, interview, discussion, etc.
                - **Participants**: Who is speaking, their roles and relationships
                - **Context**: What is being discussed, the purpose and setting
                - **Key Topics**: Main subjects, decisions, action items
                - **Tone & Style**: Formal/informal, technical level, cultural context

                ### STEP 2: VIDEO CONTEXT ANALYSIS (if video provided)
                If video content is available, analyze:
                - **Visual Context**: Setting, environment, participants' appearance
                - **Non-verbal Cues**: Body language, facial expressions, gestures
                - **Visual Elements**: Slides, documents, charts, or other visual aids
                - **Cultural Context**: Visual cues that indicate cultural or professional setting
                - **Speaker Identification**: Who is speaking based on visual cues

                ### STEP 3: VIETNAMESE TRANSCRIPT GENERATION
                Create a natural, accurate Vietnamese transcript that:
                - **Preserves Meaning**: Maintains the exact meaning and intent of the original
                - **Natural Vietnamese**: Uses appropriate Vietnamese expressions and terminology
                - **Cultural Adaptation**: Adapts cultural references and context to Vietnamese culture
                - **Professional Tone**: Matches the professional level and formality of the original
                - **Speaker Identification**: Clearly identifies who is speaking
                - **Context Preservation**: Maintains the flow and context of the conversation

                ## OUTPUT FORMAT:

                ### 📝 VIETNAMESE TRANSCRIPT

                #### 🎯 Thông tin cuộc họp
                - **Loại cuộc họp**: [Loại cuộc họp được xác định]
                - **Người tham gia**: [Danh sách người tham gia]
                - **Bối cảnh**: [Mô tả bối cảnh và mục đích]
                - **Thời gian**: [Nếu có thông tin về thời gian]

                #### 💬 Nội dung chính
                [Transcript tiếng Việt được tạo dựa trên nội dung gốc và ngữ cảnh video]

                #### ✅ Điểm quan trọng
                - [Các quyết định quan trọng]
                - [Các hành động cần thực hiện]
                - [Các vấn đề cần theo dõi]

                #### 📋 Tóm tắt
                [Tóm tắt ngắn gọn về nội dung cuộc họp]

                ## OUTPUT REQUIREMENTS:
                - Sử dụng tiếng Việt tự nhiên và chính xác
                - Giữ nguyên ý nghĩa và ngữ cảnh của bản gốc
                - Thích ứng với văn hóa Việt Nam khi cần thiết
                - Sử dụng thuật ngữ chuyên môn phù hợp
                - Định dạng rõ ràng với thông tin người nói
                - Trả về Markdown sạch không có ký tự escape

                **ENGLISH TRANSCRIPT:**
                {cleanedText}

                **VIDEO INFORMATION:**
                {(videoData != null ? $"Video file: {videoFileName} (Size: {videoData.Length} bytes)" : "No video provided")}

                Tạo transcript tiếng Việt chính xác dựa trên nội dung và ngữ cảnh được cung cấp.";

                string analysis = string.Empty;

                // 4. Gọi Gemini API
                try
                {
                    var response = await _model.GenerateContent(prompt);
                    analysis = response?.Candidates?
                        .FirstOrDefault()?
                        .Content?
                        .Parts?
                        .FirstOrDefault()?
                        .Text;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.GetType().FullName);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }

                // 5. Xử lý kết quả
                if (!string.IsNullOrEmpty(analysis))
                {
                    var cleanedAnalysis = CleanSummaryOutput(analysis);

                    // Tách analysis thành các phần
                    var vietnameseTranscript = ExtractVietnameseTranscript(cleanedAnalysis);
                    var meetingInfo = ExtractMeetingInfo(cleanedAnalysis);
                    var fullTranscript = cleanedAnalysis;

                    return ApiResponse<SummarizeVideoTextResponse>.SuccessResponse(
                        new SummarizeVideoTextResponse 
                        { 
                            Summary = vietnameseTranscript,
                            VideoAnalysis = meetingInfo,
                            CombinedAnalysis = fullTranscript
                        },
                        "Vietnamese transcript generated successfully"
                    );
                }

                throw new Exception("Cannot analyze: Analysis result is null or empty");
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
                Console.WriteLine(argEx.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private string CleanInputText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Loại bỏ khoảng trắng thừa ở đầu và cuối
            text = text.Trim();

            // Escape dấu nháy kép để tránh lỗi JSON hoặc API
            text = text.Replace("\"", "\\\"");

            // Loại bỏ các ký tự điều khiển không cần thiết
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            // Thay thế nhiều khoảng trắng liên tiếp bằng một khoảng trắng
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Thay thế nhiều xuống hàng liên tiếp bằng một xuống hàng
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s*\n", "\n");

            // Loại bỏ khoảng trắng ở đầu mỗi dòng
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s+", "\n");

            // Chuẩn hóa dấu câu tiếng Việt - đảm bảo có khoảng trắng sau dấu câu
            text = System.Text.RegularExpressions.Regex.Replace(text, @"([.!?])([A-ZÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴÈÉẸẺẼÊỀẾỆỂỄÌÍỊỈĨÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠÙÚỤỦŨƯỪỨỰỬỮỲÝỴỶỸĐ])", "$1 $2");

            // Loại bỏ khoảng trắng thừa sau khi chuẩn hóa
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }

        private string CleanSummaryOutput(string summary)
        {
            if (string.IsNullOrEmpty(summary))
                return string.Empty;

            // Xử lý các ký tự escape từ JSON
            summary = summary.Replace("\\n", "\n");
            summary = summary.Replace("\\t", "\t");
            summary = summary.Replace("\\r", "\r");
            summary = summary.Replace("\\\"", "\"");
            summary = summary.Replace("\\'", "'");
            summary = summary.Replace("\\\\", "\\");

            // Loại bỏ các ký tự Markdown
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"#{1,6}\s*", ""); // Headers
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\*\*(.*?)\*\*", "$1"); // Bold
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\*(.*?)\*", "$1"); // Italic
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"`(.*?)`", "$1"); // Code
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\[(.*?)\]\(.*?\)", "$1"); // Links

            // Chuẩn hóa xuống hàng - giữ nguyên line breaks
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\r\n|\r|\n", "\n"); // Normalize line endings
            summary = System.Text.RegularExpressions.Regex.Replace(summary, @"\n\s*\n\s*\n+", "\n\n"); // Max 2 consecutive line breaks

            return summary.Trim();
        }

        private string ExtractVietnameseTranscript(string analysis)
        {
            // Tìm phần nội dung chính trong transcript tiếng Việt
            var transcriptMatch = System.Text.RegularExpressions.Regex.Match(analysis, @"Nội dung chính[:\s]*(.*?)(?=Điểm quan trọng|Tóm tắt|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (transcriptMatch.Success)
            {
                return transcriptMatch.Groups[1].Value.Trim();
            }
            return analysis; // Fallback to full analysis
        }

        private string ExtractMeetingInfo(string analysis)
        {
            // Tìm phần thông tin cuộc họp
            var meetingMatch = System.Text.RegularExpressions.Regex.Match(analysis, @"Thông tin cuộc họp[:\s]*(.*?)(?=Nội dung chính|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (meetingMatch.Success)
            {
                return meetingMatch.Groups[1].Value.Trim();
            }
            return "Thông tin cuộc họp không có sẵn";
        }
    }
}
