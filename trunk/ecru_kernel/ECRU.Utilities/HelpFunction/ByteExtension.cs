using System.Text;

namespace ECRU.Utilities.HelpFunction
{
    public static class ByteExtension
    {
        public static byte[] GetPart(this byte[] str, int startIndex, int Count)
        {
            if (str != null && str.Length > startIndex + Count)
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
            return Encoding.UTF8.GetBytes(input);
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
    }
}