using System;
using System.Text;

namespace ECRU.Utilities.HelpFunction
{
    public static class ByteExtension
    {
        public static byte[] GetPart(this byte[] str, int startIndex, int Count)
        {
            if (str != null && str.Length >= startIndex + Count)
            {
                var res = new byte[Count];
                for (int i = 0; i < Count; i++)
                {
                    res[i] = str[startIndex + i];
                }
                return res;
            }
            return null;
        }


        public static byte[] ToBytes(this int intValue)
        {
            byte[] bytes = new byte[4];

            bytes[0] = (byte)(intValue >> 24);
            bytes[1] = (byte)(intValue >> 16);
            bytes[2] = (byte)(intValue >> 8);
            bytes[3] = (byte)intValue;

            return bytes;
        }

        public static int ToInt(this Byte[] byteValue)
        {
            return (byteValue[0] << 24) + (byteValue[1] << 16) + (byteValue[2] << 8) + byteValue[3];
        }

        public static void Set(this byte[] str, byte[] data, int index)
        {
            if (str != null && str.Length >= data.Length + index)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    str[index + i] = data[i];
                }
            }
        }

        public static byte[] StringToBytes(this string input)
        {
            try
            {
                return Encoding.UTF8.GetBytes(input);
            }
            catch
            {
                return null;
            }
        }

        public static string GetString(this byte[] bytes)
        {
            return new string(Encoding.UTF8.GetChars(bytes));
        }

        public static bool ByteArrayCompare(this byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }


        public static string ToHex(this byte b)
        {
            const string hex = "0123456789ABCDEF";
            int lowNibble = b & 0x0F;
            int highNibble = (b & 0xF0) >> 4;
            var s = new string(new[] {hex[highNibble], hex[lowNibble]});
            return s;
        }

        public static string ToHex(this byte[] bytearray)
        {
            string returnStr = "";
            foreach (byte b in bytearray)
            {
                returnStr = returnStr + b.ToHex();
            }
            return returnStr;
        }

        public static byte[] FromHex(this string s)
        {
            int length = (s.Length + 1)/2;
            var arr1 = new byte[length];
            for (int i = 0; i < length; i++)
            {
                char sixteen = s[2*i];
                if (sixteen > '9') sixteen = (char) (sixteen - 'A' + 10);
                else sixteen -= '0';

                char ones = s[2*i + 1];
                if (ones > '9') ones = (char) (ones - 'A' + 10);
                else ones -= '0';

                arr1[i] = (byte) (16*sixteen + ones);
            }
            return arr1;
        }


        public static byte[] Add(this byte[] elements, byte[] bytes)
        {
            var result = new byte[elements.Length + bytes.Length];

            Array.Copy(elements, 0, result, 0, elements.Length);
            Array.Copy(bytes, 0, result, elements.Length, bytes.Length);

            return result;
        }

        public static long ToLong(this byte[] item, int index)
        {
            if(item.Length-index < 8) throw new IndexOutOfRangeException(); 

            long data = 0; 
            for(int i = 0; i<8; i++)
            {
                data += item[i] << (8*7 - (i * 8)); 
            }

            return data; 
        }


        public static byte[] ToByte(this long item)
        {
            byte[] data =  new byte[8]; 

            for (int i = 0; i < 8; i++)
            {
                data[i] = (byte)(item >> (8 * 7 - (i * 8)));
            }

            return data;
        }


    }
}