using Newtonsoft.Json;

namespace CreateAndValidateConnections.Models
{
    public class ZgApiResponse<TType>
    {
        [JsonProperty("result")]
        public TType Result { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
