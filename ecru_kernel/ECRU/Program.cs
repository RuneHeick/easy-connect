using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.netd;
using ECRU.Utilities;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
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

            EventBus.Publish(new NewConnectionMessage { ConnectionCallback = test, ConnectionType = "hello", Receiver = "B3E795DE1C11".FromHex(), Sender = "B3E795DE1C11".FromHex()});

            Thread.Sleep(Timeout.Infinite);
        }

        public static void test(Socket s)
        {
            var connectioninfo = s.RemoteEndPoint as IPEndPoint;
            Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);
        }
    }
}