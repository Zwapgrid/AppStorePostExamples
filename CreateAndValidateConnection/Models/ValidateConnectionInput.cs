using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class ValidateConnectionInput
    {
        [JsonProperty("partnerToken")]
        public string PartnerToken { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
