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
using ECRU.SystemInfo;

namespace ECRU
{
    /// <summary>
    ///     ECRU kernel
    /// </summary>
    public class Program
    {

        private static Netd _netDaemon = new Netd();
        private static BLEModule _bleModule = null;

        private static event SystemInfoChanged systemInfoChanged;
        private static Thread mainThread;

        private static int state = 0;

        /// <summary>
        ///     Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            //Debug.EnableGCMessages(true);

            // write your code here
            //Thread.Sleep(5000);

            
            //State 1 Load SystemInfo configuration
            //GetSystemInfoConfig();

            
            systemInfoChanged += (StateSwitch);

            mainThread = Thread.CurrentThread;

            while (true)
            {
                switch (state)
                {
                    case 1: //Load System Information state
                        SystemInfo.SystemInfo.LoadConfig("SysInfoConfig.cfg");
                        SystemInfo.SystemInfo.Start();
                        break;

                    case 2: //System Configuration state
                        _bleModule.LoadConfig("");
                        _bleModule.Start();
                        break;

                    case 3: //Normal state
                        _bleModule.Reset();
                        _netDaemon.LoadConfig("");
                        _netDaemon.Start();
                        break;
                }

                Thread.Sleep(Timeout.Infinite);
            }
            

            //try
            //{
            //    _netDaemon.LoadConfig("");

            //    var tmp = IPAddress.GetDefaultLocalAddress().ToString().Split('.');
            //    var n = "ECEC";
            //    foreach (string s in tmp)
            //    {
            //        n += s;
            //    }
            //    n = n.Substring(n.Length - 12, 12);
            //    SystemInfo.SystemInfo.SystemMAC = n.FromHex();

            //    //SystemInfo.SystemMAC = "B3E795111C11".FromHex();

            //    //SystemInfo.ConnectedDevices.Add();
            //    //

            //    Debug.Print(SystemInfo.SystemMAC.ToHex());
            //    _netDaemon.Start();
            //}
            //catch (Exception exception)
            //{
            //    _netDaemon.Stop();
            //    Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
            //}

            //(new BLEModule()).Start();

            //SystemInfo.ConnectedDevices.Add("E68170E5C578".FromHex());
            //SystemInfo.ConnectedDevices.Add("F83A228CBA1C".FromHex());

            
        }

        private static void StateSwitch(Byte[] sysMac, string name, string passCode)
        {
            if (sysMac != null && name != null && passCode != null)
            {
                //System configured
                state = 3;
            } else if (state == 1)
            {
                state = 2;
            }
            if (mainThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                mainThread.Resume();
            }
            
        }
    }
}