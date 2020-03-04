using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class ValidateConnectionOutput
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
