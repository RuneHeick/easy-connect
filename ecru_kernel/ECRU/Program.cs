using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ECRU.BLEController;
using ECRU.File;
using ECRU.File.Files;
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
            Debug.EnableGCMessages(true);
            // write your code here
            Thread.Sleep(5000);

            
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            
            try
            {
                _netDaemon.LoadConfig("");

                var tmp = IPAddress.GetDefaultLocalAddress().ToString().Split('.');
                string n = "ECEC";
                foreach (string s in tmp)
                {
                    n += s;
                }

                SystemInfo.SystemMAC = n.FromHex();

                //SystemInfo.SystemMAC = "B3E795111C11".FromHex();

                //SystemInfo.ConnectedDevices.Add();
                //

                Debug.Print(SystemInfo.SystemMAC.ToHex());
                _netDaemon.Start();
            }
            catch (Exception exception)
            {
                _netDaemon.Stop();
                Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
            }

            (new BLEModule()).Start();

            SystemInfo.ConnectedDevices.Add("E68170E5C578".FromHex());

            SystemInfo.ConnectedDevices.Add("F83A228CBA1C".FromHex());

            //(new Thread(() =>
            //{
            //    FileBase file = FileSystem.GetFile("E68170E5C578.val", FileAccess.ReadWrite, FileType.Local);
            //    ECRU.BLEController.Util.DeviceInfoValueFile t = new ECRU.BLEController.Util.DeviceInfoValueFile(file);

            //    t.Update(35, new byte[] { 0x22, 0x22, 0xEC, 0xEC });
            //    t.Close();
            //})).Start();

            //Thread.Sleep(15000);

            //EventBus.Publish(new NewConnectionMessage { ConnectionCallback = test3, ConnectionType = "SetECMData", Receiver = SystemInfo.SystemMAC });

            //Thread.Sleep(15000);
            //EventBus.Publish(new NewConnectionMessage { ConnectionCallback = test2, ConnectionType = "RequestECMData", Receiver = SystemInfo.SystemMAC });

            //while (true)
            //{
            //    Thread.Sleep(10000);
            //    SystemInfo.ConnectedDevices.Add("E68170E5C578".FromHex());

            //    Thread.Sleep(10000);
            //    SystemInfo.ConnectedDevices.add("F83A228CBA1C".FromHex());
            //}
            
            Thread.Sleep(Timeout.Infinite);
        }

        public static void test(Socket s, byte[] receiver)
        {
            using (s)
            {
                try
                {
                    var connectioninfo = s.RemoteEndPoint as IPEndPoint;
                    if (connectioninfo != null)
                        Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);

                    s.Send("E68170E5C578".FromHex());
                    
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
                                Debug.Print("Got device information");
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

        public static void test2(Socket s, byte[] receiver)
        {
            using (s)
            {
                try
                {
                    var connectioninfo = s.RemoteEndPoint as IPEndPoint;
                    if (connectioninfo != null)
                        Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);

                    s.Send("E68170E5C5780023".FromHex());

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
                                Debug.Print("Got device information");
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

        public static void test3(Socket s, byte[] receiver)
        {
            using (s)
            {
                try
                {
                    var connectioninfo = s.RemoteEndPoint as IPEndPoint;
                    if (connectioninfo != null)
                        Debug.Print("Connected to: " + connectioninfo.Address + ":" + connectioninfo.Port);

                    s.Send("E68170E5C5780023ECECECEC".FromHex());

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