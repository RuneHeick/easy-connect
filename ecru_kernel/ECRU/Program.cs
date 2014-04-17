using System.Threading;
using ECRU.netd;
<<<<<<< .mine
=======
using ECRU.BLEController;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
>>>>>>> .r104

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
<<<<<<< .mine
=======

            


            BLEModule Test = new BLEModule();
            Test.Start();


            /*
>>>>>>> .r104
            var netDaemon = new Netd();
            netDaemon.LoadConfig("");
            netDaemon.Start();


            Thread.Sleep(Timeout.Infinite);
        }
    }
}