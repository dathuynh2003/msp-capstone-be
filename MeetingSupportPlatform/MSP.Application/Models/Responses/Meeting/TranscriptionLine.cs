using Newtonsoft.Json;

namespace MSP.Application.Models.Responses.Meeting
{
    public class TranscriptionLine
    {
        [JsonProperty("speaker_id")]
        public string SpeakerId { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("start_ts")]
        public int StartTs { get; set; }

        [JsonProperty("stop_ts")]
        public int StopTs { get; set; }
    }

}
