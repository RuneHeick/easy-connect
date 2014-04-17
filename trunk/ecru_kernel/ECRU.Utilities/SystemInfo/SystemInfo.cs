using System;
using Microsoft.SPOT;
using System.Text; 

namespace ECRU.Utilities
{
    public static class SystemInfo
    {
        public const int SYSID_LENGTH = 20;
        static string passCode = "";
        static byte[] sysId = new byte[SYSID_LENGTH];

        public static byte[] SystemID
        {
            get
            {
                return sysId;
            }
        }

        public static string PassCode
        {
            get
            {
                return passCode;
            }
            set
            {
                passCode = value;
                sysId = doHash(passCode);
            }
        }

        public static string Name { get; set; }

        static byte[] doHash(string input)
        {
            /*
            HashAlgorithm Hash = new HashAlgorithm(HashAlgorithmType.SHA1);
            return Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            */
            return new byte[20];
        }




    }
}
