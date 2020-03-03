using Newtonsoft.Json;

namespace AppStorePostExamples.Models
{
    public class Connection
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("id")]
        public int? Id { get; set; }

        // TODO add more properties here
    }
}
