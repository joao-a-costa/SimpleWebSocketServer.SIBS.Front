using Newtonsoft.Json;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class ReceiptTextsData
    {
        [JsonProperty("acquirerText")]
        public string AcquirerText { get; set; }
    }
}
