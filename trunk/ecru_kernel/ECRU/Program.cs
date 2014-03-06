using System.IO;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using ECRU.netd;
using Microsoft.SPOT;
using ECRU.SD;

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

            Debug.Print("are we live?");

            var sd = new SD.SD();
            var netd = new Netd();

            var message = new GetNetworkProfilMessage("SD\\Config\\netd.txt");

            EventBus.EventBus.Publish(message);

        }
    }
}