using Mscc.GenerativeAI;
using MSP.Shared.Common;
using MSP.Application.Models.Responses.Summarize;
using MSP.Application.Services.Interfaces.Summarize;

namespace MSP.Infrastructure.Processors
{
    public class GeminiTextSummarizer : IGeminiTextSummarizer
    {
        private readonly GenerativeModel _model;

        public GeminiTextSummarizer(GenerativeModel model)
        {
            _model = model;
        }

        public async Task<ApiResponse<SummarizeTextResponse>> SummarizeAsync(string text)
        {
            try
            {
                // 1. Kiểm tra đầu vào
                if (text == null)
                    throw new Exception("Input cannot be null or empty");

                // 2. Làm sạch text đầu vào
                var cleanedText = CleanInputText(text);
                if (string.IsNullOrWhiteSpace(cleanedText))
                    throw new Exception("Input is empty or not available");

                // 3. Tạo prompt cho summarize
                var prompt = @$"You are an advanced AI assistant with expertise in intelligent text analysis and professional summarization. Your task is to analyze the following text and create a comprehensive summary that adapts to the content type and context. Always respond in the SAME LANGUAGE as the input text.

                ## ANALYSIS INSTRUCTIONS:

                ### STEP 1: CONTENT TYPE DETECTION
                Analyze the text to determine its type:
                - **Meeting Transcript**: Contains dialogue, discussions, decisions, action items, participants
                - **Document/Report**: Structured information, data, analysis, findings
                - **Email/Communication**: Messages, requests, updates, notifications
                - **Technical Content**: Code, specifications, procedures, technical discussions
                - **General Text**: Any other content type

                ### STEP 2: ADAPTIVE SUMMARIZATION

                **For MEETING TRANSCRIPTS:**
                ```
                ## 📋 MEETING SUMMARY

                ### 🎯 Meeting Overview
                - **Topic**: [Main purpose/agenda]
                - **Date/Time**: [Extract if available]
                - **Participants**: [Key attendees/roles]
                - **Duration**: [If mentioned]

                ### 💬 Key Discussions
                - [Main topics debated]
                - [Different viewpoints expressed]
                - [Concerns raised]
                - [Questions asked]

                ### ✅ Decisions & Outcomes
                - **Decisions Made**: [Final agreements]
                - **Action Items**: [Specific tasks with owners]
                - **Deadlines**: [Time commitments]
                - **Next Steps**: [Follow-up actions]

                ### ⚠️ Risks & Dependencies
                - [Potential blockers]
                - [Resource constraints]
                - [External dependencies]

                ### 🔄 Follow-up Required
                - [Items needing confirmation]
                - [Scheduled next meetings]
                - [Pending information]
                ```

                **For DOCUMENTS/REPORTS:**
                ```
                ## 📄 DOCUMENT SUMMARY

                ### 📊 Executive Summary
                [Key findings and main conclusions]

                ### 🔍 Key Points
                - [Main arguments/points]
                - [Supporting evidence]
                - [Data/statistics mentioned]

                ### 💡 Insights & Analysis
                - [Critical insights]
                - [Trends identified]
                - [Implications]

                ### 🎯 Recommendations
                - [Suggested actions]
                - [Next steps]
                - [Priority items]
                ```

                **For EMAILS/COMMUNICATIONS:**
                ```
                ## 📧 COMMUNICATION SUMMARY

                ### 📨 Message Overview
                - **From/To**: [Sender and recipients]
                - **Subject**: [Main topic]
                - **Purpose**: [Request/update/information]

                ### 📝 Key Information
                - [Main points communicated]
                - [Requests made]
                - [Updates provided]

                ### ⚡ Action Required
                - [Response needed]
                - [Deadlines]
                - [Follow-up actions]
                ```

                **For TECHNICAL CONTENT:**
                ```
                ## 🔧 TECHNICAL SUMMARY

                ### 🎯 Purpose & Scope
                [What the content addresses]

                ### 🛠️ Technical Details
                - [Key technical points]
                - [Specifications/requirements]
                - [Implementation details]

                ### ⚠️ Important Considerations
                - [Limitations/constraints]
                - [Dependencies]
                - [Best practices mentioned]

                ### 🚀 Next Steps
                - [Implementation tasks]
                - [Testing requirements]
                - [Documentation needs]
                ```

                **For GENERAL TEXT:**
                ```
                ## 📖 CONTENT SUMMARY

                ### 🎯 Main Topic
                [Primary subject matter]

                ### 🔑 Key Points
                - [Main ideas presented]
                - [Supporting arguments]
                - [Important details]

                ### 💭 Key Insights
                - [Critical takeaways]
                - [Cause-effect relationships]
                - [Comparisons made]

                ### 📋 Summary
                [Concise overview of the entire content]
                ```

                ## OUTPUT REQUIREMENTS:
                - Use appropriate emojis for visual clarity
                - Maintain professional tone
                - Focus on actionable information
                - Preserve important details
                - Use bullet points for readability
                - Keep language consistent with input
                - Return clean Markdown without escape characters
                - Output language: Vietnamese.
                **INPUT TEXT:**
                {cleanedText}

                Analyze the text and provide a summary using the appropriate format based on the content type detected.";

                string summary = string.Empty;

                // 4. Gọi Gemini API
                try
                {
                    var response = await _model.GenerateContent(prompt);
                    summary = response?.Candidates?
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
                if (!string.IsNullOrEmpty(summary))
                {
                    var cleanedSummary = CleanSummaryOutput(summary);

                    return ApiResponse<SummarizeTextResponse>.SuccessResponse(
                        new SummarizeTextResponse { Summary = cleanedSummary },
                        "Summarize successfully"
                    );
                }

                throw new Exception("Cannot summarize: Summary text is null or empty");
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

        public async Task<ApiResponse<SummarizeTextResponse>> GenerateTodoListAsync(string text)
        {
            try
            {
                // 1. Kiểm tra đầu vào
                if (text == null)
                    throw new Exception("Input cannot be null or empty");

                // 2. Làm sạch text đầu vào
                var cleanedText = CleanInputText(text);
                if (string.IsNullOrWhiteSpace(cleanedText))
                    throw new Exception("Input is empty or not available");

                // 3. Tạo prompt cho todo list
                var prompt = @$"
                You are an AI assistant. Your task is to extract a TASK LIST from the following meeting transcript.
                Requirements:
                - Return output as a valid JSON array.
                - Each task must be an object with the following fields:
                  - assignee (string, leave empty if not provided)
                  - startDate (string, format dd-MM-yyyy or empty if not provided)
                  - endDate (string, format dd-MM-yyyy or empty if not provided)
                  - priority (string: High, Medium, Low or empty if not provided)
                - Only extract tasks, no explanations, no extra text.
                - Output language for assignee and task content: Vietnamese.

                Meeting transcript:
                {cleanedText}

                Output:
                ";

                string summary = string.Empty;

                // 4. Gọi Gemini API
                try
                {
                    var response = await _model.GenerateContent(prompt);
                    summary = response?.Candidates?
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
                if (!string.IsNullOrEmpty(summary))
                {
                    var cleanedSummary = CleanSummaryOutput(summary);

                    return ApiResponse<SummarizeTextResponse>.SuccessResponse(
                        new SummarizeTextResponse { Summary = cleanedSummary },
                        "Todo list generated successfully"
                    );
                }

                throw new Exception("Cannot generate todo list: Output text is null or empty");
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
    }
}
