using System.Threading;
using ECRU.netd;

namespace ECRU
{
    /// <summary>
    ///     ECRU kernel
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            // write your code here

            var netDaemon = new Netd();
            netDaemon.LoadConfig("");
            netDaemon.Start();


            Thread.Sleep(Timeout.Infinite);
        }
    }
}