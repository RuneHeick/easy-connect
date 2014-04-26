using System;
namespace ECRU.Utilities
{
    public static class SystemInfo
    {
        public const int SYSID_LENGTH = 8;
        private static string passCode = "";
        private static byte[] sysId = new byte[SYSID_LENGTH];
        private static byte[] sysMac;

        public static MacHierarchy ConnectionOverview { get; private set; }

        public static MacList ConnectedDevices { get; private set; }


        public static byte[] SystemID
        {
            get { return sysId; }
        }

        public static string PassCode
        {
            get { return passCode; }
            set
            {
                passCode = value;
                sysId = doHash(passCode);
            }
        }

        public static string Name { get; set; }

        static SystemInfo()
        {
            Init();
        }


        public static void Init()
        {
            if (SystemMAC != null)
            {
                ConnectionOverview = new MacHierarchy();
                ConnectionOverview.Add(SystemMAC);
                ConnectedDevices = ConnectionOverview.GetDecices(SystemMAC);
            }
        }

        public static byte[] SystemMAC
        {
            get { return sysMac; }
            set
            {
                sysMac = value;
                Init();
            }
        }

        private static byte[] doHash(string input)
        {
            UInt64 hashedValue = HelpFunction.Knuthhash.doHash(input);

            byte[] hash = new byte[SYSID_LENGTH];
            for (int i = 0; i < 64 / 8; i++)
            {
                hash[i] = (byte)(hashedValue >> (8 * i));
            }
            return hash;
        }

    }
}