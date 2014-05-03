using System;
using System.Text;

namespace ECRU.netd
{
    class TimeOutException : Exception
    {
        public override string Message
        {
            get
            {
                return "Timed out";
            }
        }
    }
}
