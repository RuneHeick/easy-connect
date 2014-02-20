using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using logging;

namespace ecru_kernel
{
    public class Program
    {
        public static void Main()
        {
            // write your code here
            
            Debug.Print("are we live?");

            foreach (var folders in Directory.GetDirectories(""))
            {
                Debug.Print(folders);
            }
           

            // Directly start logging, no need to create any instance of Logger class
            Logging.LogToFile = true;    // if false it will only do Debug.Print()
            Logging.Append = true;       // will append the information to existing if any
            Logging.PrefixDateTime = true; // add a time stamp on each Log call. Note: Netduino time is not same as clock time.

            Logging.Log("lort is good!");

        }
    }
}
