using System;
using Microsoft.SPOT;

namespace ECRU.BLEController
{
    static class Def
    {
        public const int COMMAND_INDEX = 2;
        public const int SYNC_WORD = 0xEC;
        public const int MAX_PACKETSIZE = 128;
        public const int STARTFIELD_INDEX = 1;
        public const int STARTFIELD_MASK = 0x80;
        public const int RETRANSMITTIME = 100;
        public const int RETRANSMIT_MAXCOUNT = 3;

        public const UInt16 MAIN_SERVICE_UUID       = 0x1800;
        public const UInt16 PRIMSERVICE_NAME_UUID   = 0x2A00;

        //----------------------------------------------------------
        public const UInt16 ECCONNECT_UUID          = 0x1821;

        public const UInt16 SYSTEMID_CHARA_UUID     = 0x2A73;  // Custom EC System ID 
        public const UInt16 UPDATETIME_CHARA_UUID   = 0x2A74;  // Update Time

        //----------------------------------------------------------
        public const UInt16 DEVICEINFO_UUID         = 0x180A;

        public const UInt16  MODEL_NUMBER_UUID      = 0x2A24 ; // Model Number String
        public const UInt16  SERIAL_NUMBER_UUID     = 0x2A25;  // Serial Number String
        public const UInt16  MANUFACTURER_NAME_UUID = 0x2A29; // Manufacturer Name String

        //----------------------------------------------------------
        public const UInt16 ECSERVICE_UUID          = 0x1820;
        public const UInt16 UPDATE_UUID             = 0x2A71;
        public const UInt16 EC_DESCRIPTION_UUID     = 0x2A72;

        public const UInt16 GENERIC_VALUE_UUID      = 0x2A70;
        public const UInt16 DESCRIPTION_UUID        = 0x2901;
        public const UInt16 FORMAT_UUID             = 0x2904;
        public const UInt16 GUIFORMAT_UUID          = 0x2910;

        public const UInt16 RANGE_UUID              = 0x2906;
        public const UInt16 SUPSCRIPTIONOPTION_UUID = 0x2911;

        public const int SCAN_TIMEOUT = 15000;
        public const int READPROP = 2;


        internal static bool IsHandleUUID(UInt16 UUID)
        {
            return UUID == ECSERVICE_UUID || UUID == ECCONNECT_UUID || UUID == DEVICEINFO_UUID || UUID == MAIN_SERVICE_UUID; 
        }

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
        NameEvent = 0xA1,
        PassCode = 0xA2,
        Info = 0xA0,
        AddrEvent = 0xE0,
        Reset = 0xF0,
    }



}
