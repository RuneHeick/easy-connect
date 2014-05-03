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
        private static BLEModule _bleModule = null;
        
        

        /// <summary>
        ///     Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            Debug.EnableGCMessages(true);

            // write your code here
            Thread.Sleep(5000);

            
            //State 1 Load SystemInfo configuration
            //GetSystemInfoConfig();
            

            try
            {
                _netDaemon.LoadConfig("");

                var tmp = IPAddress.GetDefaultLocalAddress().ToString().Split('.');
                var n = "ECEC";
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

            Thread.Sleep(Timeout.Infinite);
        }

        private static void GetSystemInfoConfig()
        {
            var SysConfigFileBase = FileSystem.GetFile("SystemInfo.cfg", FileAccess.Read, FileType.Local);

            if (SysConfigFileBase != null)
            {
                var sysConfigFile = new ConfigFile(SysConfigFileBase);

                SystemInfo.SystemMAC = sysConfigFile["SystemMac"].FromHex();
                SystemInfo.Name = sysConfigFile["Name"];
                SystemInfo.PassCode = sysConfigFile["PassCode"];

                
            }
            else
            {
                SysConfigFileBase = FileSystem.CreateFile("SystemInfo.cfg", FileType.Local);
            }
            
        }
    }
}