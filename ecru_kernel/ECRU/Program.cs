using System;
using System.Threading;
using ECRU.BLEController;
using ECRU.netd;
using ECRU.Utilities;
using ECRU.Utilities.HelpFunction;

namespace ECRU
{
    /// <summary>
    ///     ECRU kernel
    /// </summary>
    public class Program
    {
        private static readonly Netd _netDaemon = new Netd();
        private static readonly BLEModule _bleModule = new BLEModule();
        private static readonly object _stateLock = new object(); 

        private static Thread mainThread;

        private static int currentState = 0;
        private static int newState = 1;
        /// <summary>
        ///     Main Launches the ECRU kernel
        /// </summary>
        public static void Main()
        {
            //Debug.EnableGCMessages(true);

            // write your code here
            Thread.Sleep(5000);


            //State 1 Load SystemInfo configuration
            //GetSystemInfoConfig();


            SystemInfo.SysInfoChange += (StateSwitch);

            mainThread = Thread.CurrentThread;

            while (true)
            {
                if (currentState != newState)
                {
                    lock (_stateLock)
                    {
                        currentState = newState;
                    }

                    switch (currentState)
                    {
                        case 1: //Load System Information state
                            SystemInfo.LoadConfig("SysInfoConfig.cfg");
                            SystemInfo.Start();
                            SystemInfo.ConnectedDevices.Add("E68170E5C578".FromHex());
                            SystemInfo.ConnectedDevices.Add("F83A228CBA1C".FromHex());
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
                }
                else
                {
                    mainThread.Suspend();
                }

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

            
        }

        private static void StateSwitch(Byte[] sysMac, string name, string passCode)
        {
            bool swichstate = false; 
            if (sysMac != null && name != null && passCode != null && currentState != 3)
            {
                //System configured
                lock (_stateLock)
                {
                    newState = 3;
                }
                    swichstate = true; 
            }
            else if (currentState == 1 && currentState != 2)
            {
                lock (_stateLock)
            {
                    newState = 2;
                }
                    swichstate = true; 
            }

            if (swichstate == true)
            {
                    if ((mainThread.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
            {
                mainThread.Resume();
            }
        }
    }
}
}