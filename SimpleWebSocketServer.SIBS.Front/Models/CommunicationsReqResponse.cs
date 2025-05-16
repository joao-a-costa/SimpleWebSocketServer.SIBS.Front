using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class CommunicationsReqResponse
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Version Version { get; set; } = Version.V_1;

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; } = RequestType.COMMUNICATIONS_RESPONSE;

        [JsonProperty("genericCommunicationResult")]
        public bool GenericCommunicationResult { get; set; }

        [JsonProperty("genericCommunicationTime")]
        public int GenericCommunicationTime { get; set; }

        [JsonProperty("qrcodeConnectionTime")]
        public int QrcodeConnectionTime { get; set; }

        [JsonProperty("qrcodeInquiryCommunicationResult")]
        public bool QrcodeInquiryCommunicationResult { get; set; }
    }
}
