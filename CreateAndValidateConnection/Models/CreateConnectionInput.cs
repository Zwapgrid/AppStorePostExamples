using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class CreateConnectionInput
    {
        [JsonProperty("partnerToken")]
        public string PartnerToken { get; set; }
        [JsonProperty("connection")]
        public Connection Connection { get; set; }
    }
}
