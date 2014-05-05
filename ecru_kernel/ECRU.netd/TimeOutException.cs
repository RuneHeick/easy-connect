using System;

namespace ECRU.netd
{
    internal class TimeOutException : Exception
    {
        public override string Message
        {
            get { return "Timed out"; }
        }
    }
}