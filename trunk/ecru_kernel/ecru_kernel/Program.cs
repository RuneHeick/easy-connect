using System;
using System.Collections;
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

        delegate void func1delegate(string input);

        static Hashtable GetHashtable()
        {
            // Create and return new Hashtable.
            Hashtable hashtable = new Hashtable();

            var list = new ArrayList { new func1delegate(func1), new func1delegate(func2) };

            hashtable.Add("LogEvent", list);
            hashtable.Add("Mortgage", 540);

            list.Add(new func1delegate(func1));
            return hashtable;
        }

        static void func1(string s)
        {
            Debug.Print("func 1! " + s);
        }

        static void func2(string s)
        {
            Debug.Print("func 2! " + s);
        }

        public static void Main()
        {
            // write your code here
            
            Debug.Print("are we live?");

            // Directly start logging, no need to create any instance of Logger class
            Logging.LogToFile = true;    // if false it will only do Debug.Print()
            Logging.Append = true;       // will append the information to existing if any
            Logging.PrefixDateTime = true; // add a time stamp on each Log call. Note: Netduino time is not same as clock time.

            Logging.Log("lort is good!");
            Logging.Log("lort is good!");
            Logging.Log("lort is good!");
            Logging.Log("lort is good!");


            Logging.Close();

            var hashtable = GetHashtable();

            var list = hashtable["LogEvent"] as ArrayList;

            if (list == null) return;
            foreach (func1delegate s in list)
            {
                s.Invoke("LogEvent");
            }
        }
    }
}
