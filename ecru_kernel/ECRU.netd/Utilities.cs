using System;
using System.Text;
using Microsoft.SPOT;

namespace ECRU.netd
{
    public static class Utilities
    {  
        public static byte[] StringToBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string GetString(this byte[] bytes)
        {
            return new string(Encoding.UTF8.GetChars(bytes));
        }
    }
}
