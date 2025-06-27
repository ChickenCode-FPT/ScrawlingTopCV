using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Models
{
    public class StartProfileData
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("profile_id")]
        public int Profile_Id { get; set; }

        [JsonPropertyName("browser_location")]

        public string BrowserLocation { get; set; } = string.Empty;

        [JsonPropertyName("remote_debugging_address")]

        public string RemoteDebuggingAddress { get; set; } = string.Empty;

        [JsonPropertyName("driver_path")]

        public string DriverPath { get; set; } = string.Empty;
    }
}
