using System.IO;
using System.Threading;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using ECRU.netd;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace ECRU
{
    /// <summary>
    /// ECRU kernel
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            // write your code here

            var netDaemon = new Netd();

            netDaemon.LoadConfig("");
            netDaemon.Start();
        }
    }
}