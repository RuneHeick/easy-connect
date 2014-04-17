using System;
namespace ECRU.Utilities
{
    public static class SystemInfo
    {
        public const int SYSID_LENGTH = 20;
        private static string passCode = "";
        private static byte[] sysId = new byte[SYSID_LENGTH];

        static SystemInfo()
        {
            ConnectionOverview = new MacHierarchy();
        }

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

        public static byte[] SystemMAC { get; set; }

        private static byte[] doHash(string input)
        {
            UInt64 hashedValue = HelpFunction.Knuthhash.doHash(input);
           
            byte[] hash = new byte[20];
            for (int i = 0; i < 64 / 8; i++)
            {
                hash[i] = (byte)(hashedValue >> (8 * i));
            }
            return hash;
        }

    }
}