using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class ValidateConnectionInput
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
