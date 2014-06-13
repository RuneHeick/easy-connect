using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkAnalysor
{
    class ByteComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            return ECMList.ArraysEqual(x, y); 
        }
        public int GetHashCode(byte[] codeh)
        {
            return codeh.GetHashCode();
        }
    }
}
