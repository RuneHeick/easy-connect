using System;
using Microsoft.SPOT;

namespace ECRU.Utilities.EventBus.Events
{
    public class NetworkStatusMessage
    {
        public string NetState { get; set; }
        public bool isinsync { get; set; }
    }
}
