using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UartTester.Helper
{
    class Debug
    {
        public static ObservableCollection<string> Console { get; set; }
        static Dictionary<string, Func<string, string>> Actions { get; set; }

        static Debug()
        {
            Console = new ObservableCollection<string>();
            Actions = new Dictionary<string, Func<string, string>>();
            AddFunction("calls", Allcalls);
        }

        public static void WriteLine(string Text)
        {
#if DEBUG 
            Console.Add(Text); 
#endif
        }

        public static bool AddFunction(string CallName, Func<string, string> func)
        {
            if (Actions.ContainsKey(CallName))
                return false;
            Actions.Add(CallName, func);
            return true;
        }

        public static void AddCall(string Message)
        {
            Console.Add(Message);
            string[] calls = Actions.Keys.ToArray();
            foreach (string call in calls)
            {
                if (Message.StartsWith(call))
                {
                    Console.Add("Running Function: " + call);
                    string ret = Actions[call](Message.Length > call.Length ? Message.Substring(call.Length) : "");
                    Console.Add(ret);
                    break;
                }
            }
        }

        public static string Allcalls(string input)
        {
            string ret = "";
            string[] calls = Actions.Keys.ToArray();
            foreach (string call in calls)
            {
                ret = ret + call + "\n";
            }
            return ret;
        }

    }


}
