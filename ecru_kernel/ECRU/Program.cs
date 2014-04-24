using System;
using System.Threading;
using ECRU.netd;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

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
            Thread.Sleep(5000);
            SystemInfo.SystemMAC = "B3E795DE1C11".FromHex();

            var netDaemon = new Netd();
            try
            {
                netDaemon.LoadConfig("");
            }
            catch (Exception exception)
            {
                Debug.Print("Network Config error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                throw;
            }
            
            
            netDaemon.Start();


            Thread.Sleep(Timeout.Infinite);
        }
    }
}