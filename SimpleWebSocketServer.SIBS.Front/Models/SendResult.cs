using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimpleWebSocketServer.SIBS.Front.Enums.Enums;

namespace SimpleWebSocketServer.SIBS.Front.Models
{
    public class SendResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public StatusCode StatusCode { get; set; } = StatusCode.OK;

    }
}
