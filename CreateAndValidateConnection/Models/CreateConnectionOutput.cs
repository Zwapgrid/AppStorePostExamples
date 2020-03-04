using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class CreateConnectionOutput
    {
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("connection")]
        public Connection Connection { get; set; }
    }
}
