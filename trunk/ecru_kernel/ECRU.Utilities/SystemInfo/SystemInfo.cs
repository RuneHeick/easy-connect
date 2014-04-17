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


        public static byte[] SystemMAC { get; set; }

        private static byte[] doHash(string input)
        {
            /*
            HashAlgorithm Hash = new HashAlgorithm(HashAlgorithmType.SHA1);
            return Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            */
            return new byte[20];
        }
    }
}