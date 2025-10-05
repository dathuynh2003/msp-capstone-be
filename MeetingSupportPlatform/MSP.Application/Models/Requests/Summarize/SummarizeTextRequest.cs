using System.ComponentModel.DataAnnotations;

namespace MSP.Application.Models.Requests.Summarize
{
    public class SummarizeTextRequest
    {
        [Required(ErrorMessage = "Text is required")]

        public string Text { get; set; }
    }
}
