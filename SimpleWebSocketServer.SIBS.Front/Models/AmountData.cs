using Newtonsoft.Json;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class AmountData
    {
        [JsonProperty("otherAmountType")]
        public string OtherAmountType { get; set; } = "NONE";

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("otherAmount")]
        public double OtherAmount { get; set; }
    }
}
