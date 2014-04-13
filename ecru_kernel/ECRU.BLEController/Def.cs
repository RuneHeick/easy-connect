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
        AddDeviceEvent = 0x10,
        DataEvent = 0x50,
        DeviceEvent =0x70,
        DisconnectEvent = 0x60,
        ReadEvent = 0x30,
        WriteEvent = 0x40,
        ServiceEvent = 0x20,
        SystemInfo = 0x80, 
        DiscoverEvent = 0x90,
        Reset = 0xF0,
    }



}
