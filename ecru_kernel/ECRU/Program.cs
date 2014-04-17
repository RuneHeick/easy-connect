using System.IO;
using System.Threading;
using ECRU.EventBus;
using ECRU.EventBus.Messages;
using ECRU.netd;
using ECRU.BLEController;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System;


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

            


            BLEModule Test = new BLEModule();
            Test.Start();

            try
            {
                ECRU.Utilities.SystemInfo.ConnectionOverview.UnitAdded += ConnectionOverview_UnitAdded;
                ECRU.Utilities.SystemInfo.ConnectionOverview.UnitRemoved += ConnectionOverview_UnitRemoved;
                ECRU.Utilities.SystemInfo.ConnectionOverview.Add(new byte[6], new byte[6]);

                ECRU.Utilities.SystemInfo.ConnectionOverview.Add(new byte[6], new byte[6] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 });

                ECRU.Utilities.MacList m = ECRU.Utilities.SystemInfo.ConnectionOverview.GetDecices(new byte[6]);

                m.Remove(new byte[6] { 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 });

                /*
                var netDaemon = new Netd();
                netDaemon.LoadConfig("");
                netDaemon.Start();
                 * }
             
                */
            }
            catch(Exception e)
            {

            }
            
            Thread.Sleep(Timeout.Infinite);
        }

        static void ConnectionOverview_UnitRemoved(byte[] master, byte[] unit)
        {
            
        }

        static void ConnectionOverview_UnitAdded(byte[] master, byte[] unit)
        {
            
        }
    }





}