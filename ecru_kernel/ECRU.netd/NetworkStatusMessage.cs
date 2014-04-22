using System;
using Microsoft.SPOT;

namespace ECRU.netd
{
    class NetworkStatusMessage
    {
        public string NetState { get; set; }
        public bool isinsync { get; set; }
    }
}
