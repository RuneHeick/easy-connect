using System.Text;

namespace ECRU.netd
{
    public static class Utilities
<<<<<<< .mine
    {
        public static byte[] StringToBytes(string input)
=======
    {  
        public static byte[] StringToBytes(this string input)
>>>>>>> .r104
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string GetString(this byte[] bytes)
        {
            return new string(Encoding.UTF8.GetChars(bytes));
        }
    }
}