using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Summarize
{
    public class SummarizeVideoTextRequest
    {
        /// <summary>
        /// Transcript tiếng Anh của video (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "English transcript is required")]
        public string Text { get; set; }

        /// <summary>
        /// File video (tùy chọn) - để cung cấp ngữ cảnh visual
        /// </summary>
        public IFormFile? Video { get; set; }
    }
}
