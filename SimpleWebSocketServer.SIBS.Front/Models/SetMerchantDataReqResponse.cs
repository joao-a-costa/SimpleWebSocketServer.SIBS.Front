using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class SetMerchantDataReqResponse
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Version Version { get; set; } = Version.V_1;

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; } = RequestType.SET_MERCHANT_DATA_RESPONSE;

        [JsonProperty("merchantData")]
        public MerchantData MerchantData { get; set; }
    }
}
