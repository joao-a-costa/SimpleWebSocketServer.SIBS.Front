using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class DeletePendingReversalsReqResponse
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Version Version { get; set; } = Version.V_1;

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; } = RequestType.DELETE_PENDING_REVERSALS_RESPONSE;

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
