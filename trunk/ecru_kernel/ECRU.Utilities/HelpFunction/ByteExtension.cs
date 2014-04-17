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
    }
}