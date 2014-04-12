using System;
using Microsoft.SPOT;

namespace ECRU.BLEController
{
    static class Def
    {
        public const int COMMAND_INDEX = 2;
        public const int SYNC_WORD = 0xEC;
        public const int MAX_PACKETSIZE = 128;

    }

    public enum CommandType
    {
        AddDeviceEvent = 0x1,
        DataEvent = 0x5,
        DeviceEvent =0x7,
        DisconnectEvent = 0x6,
        ReadEvent = 0x3,
        WriteEvent = 0x4,
        ServiceEvent = 0x2,
        SystemInfo = 0x8, 
        Reset = 0xF,
    }



}
