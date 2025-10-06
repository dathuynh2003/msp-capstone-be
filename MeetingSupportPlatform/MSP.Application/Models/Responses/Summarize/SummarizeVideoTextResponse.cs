namespace MSP.Application.Models.Responses.Summarize
{
    public class SummarizeVideoTextResponse
    {
        /// <summary>
        /// Transcript tiếng Việt được tạo dựa trên nội dung gốc
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Thông tin cuộc họp (người tham gia, bối cảnh, loại cuộc họp)
        /// </summary>
        public string VideoAnalysis { get; set; }
        
        /// <summary>
        /// Transcript đầy đủ bao gồm thông tin cuộc họp và nội dung chính
        /// </summary>
        public string CombinedAnalysis { get; set; }
    }
}
