using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Models.Requests.Meeting
{
    public class StreamUserRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("role")]
        public string Role { get; set; } = "user";

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("image")]
        public string? Image { get; set; }
    }
}
