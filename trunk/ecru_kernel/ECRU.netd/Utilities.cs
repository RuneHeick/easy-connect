using System;
using System.Collections;
using System.Net;
using System.Text;

namespace ECRU.netd
{
    public static class Utilities
    {
        //public static byte[] StringToBytes(this string input)
        //{
        //    return Encoding.UTF8.GetBytes(input);
        //}

        //public static string GetString(this byte[] bytes)
        //{
        //    return new string(Encoding.UTF8.GetChars(bytes));
        //}

        public static string GetBroadcastAddress(string ipAddress, string subnetMask)
        {
            //determines a broadcast address from an ip and subnet
            IPAddress ip = IPAddress.Parse(ipAddress);
            IPAddress mask = IPAddress.Parse(subnetMask);

            byte[] ipAdressBytes = ip.GetAddressBytes();
            byte[] subnetMaskBytes = mask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress).ToString();
        }

        public class DeviceMacList
        {
            private ArrayList _devices = new ArrayList();
            public string mac { get; set; }
            public string Name { get; set; }
            public ArrayList Devices { get { return _devices; } set { _devices = value; } }
        }
    }
}