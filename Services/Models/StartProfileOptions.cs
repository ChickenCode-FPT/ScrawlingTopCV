using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Services.Models
{
    public class StartProfileOptions
    {
        [JsonPropertyName("addination_args")] 
        public string? AdditionalArgs { get; set; } 

        [JsonPropertyName("win_pos")]
        public string? WindowPosition { get; set; } 

        [JsonPropertyName("win_size")]
        public string? WindowSize { get; set; } 

        [JsonPropertyName("win_scale")]
        public double? WindowScale { get; set; }
    }
}
