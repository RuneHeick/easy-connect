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

            
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            
            try
            {
                _netDaemon.LoadConfig("");

                var tmp = IPAddress.GetDefaultLocalAddress().ToString().Split('.');
                string n = "EC";
                foreach (string s in tmp)
                {
                    n += s;
                }

                SystemInfo.SystemMAC = n.FromHex();

                //SystemInfo.SystemMAC = "B3E795111C11".FromHex();

                //SystemInfo.ConnectedDevices.Add();
                //SystemInfo.ConnectedDevices.Add("B3E795EE1C11".FromHex());

                Debug.Print(SystemInfo.SystemMAC.ToHex());

                _netDaemon.Start();
            }
            catch (Exception exception)
            {
                _netDaemon.Stop();
                Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
            }

            

            //while (true)
            //{
            //    EventBus.Publish(new NewConnectionMessage { ConnectionCallback = test, ConnectionType = "RequestDevices", Receiver = "B3E795DE1C11".FromHex(), Sender = "B3E795DE1C11".FromHex() });
            //    Thread.Sleep(10000);
            //}
            
            Thread.Sleep(Timeout.Infinite);
        }

        public static void test(Socket s)
        {
            using (s)
            {
                try
                {
                    var connectioninfo = s.RemoteEndPoint as IPEndPoint;
                    if (connectioninfo != null)
                        Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);

                    var waitingForData = true;

                    while (waitingForData)
                    {
                        waitingForData = !s.Poll(10, SelectMode.SelectRead) && !s.Poll(10, SelectMode.SelectError);

                        if (s.Available > 0)
                        {
                            var availableBytes = s.Available;

                            var buffer = new byte[availableBytes];

                            var bytesReceived = s.Receive(buffer);

                            if (bytesReceived == availableBytes)
                            {
                                Debug.Print("Got devices" + buffer.GetString());
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                }
                finally
                {
                    if (s != null)
                    {
                        s.Close();
                    }
                }
            }
        }



    }
}