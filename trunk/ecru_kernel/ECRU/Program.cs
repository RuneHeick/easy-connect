using System;
using System.Threading;
using ECRU.netd;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;

namespace ECRU
{
    /// <summary>
    ///     ECRU kernel
    /// </summary>
    public class Program
    {

        private static Netd _netDaemon = new Netd();

        private static void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                try
                {
                    _netDaemon.LoadConfig("");
                    _netDaemon.Start();
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
                
            }
            else
            {
                try
                {
                    _netDaemon.Stop();
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
            }
        }

        /// <summary>
        ///     Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            // write your code here
            Thread.Sleep(5000);

            SystemInfo.SystemMAC = "B3E795DE1C11".FromHex();
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            
            try
            {
                _netDaemon.LoadConfig("");
                _netDaemon.Start();
            }
            catch (Exception exception)
            {
                _netDaemon.Stop();
                Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}