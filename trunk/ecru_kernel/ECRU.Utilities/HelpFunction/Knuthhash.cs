using System;

namespace ECRU.Utilities.HelpFunction
{
    public class Knuthhash
    {
        public static UInt64 doHash(string input)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < input.Length; i++)
            {
                hashedValue += input[i];
                hashedValue *= 3074457345618258799ul;
            }

            return hashedValue;
        }
    }
}