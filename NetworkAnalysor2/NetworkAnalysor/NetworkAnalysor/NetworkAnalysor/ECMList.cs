using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows; 

namespace NetworkAnalysor
{
    public class ECMList
    {
        private static List<ObservableCollection<byte[]>> AllECMLists; 
        public ObservableCollection<byte[]> ECMs { get; set;}


        public ECMList()
        {
            ECMs = new ObservableCollection<byte[]>();
            lock (AllECMLists)
                AllECMLists.Add(ECMs); 
        }

        public void Add(byte[] mac)
        {
            lock (AllECMLists)
            {
                if (!ECMs.Contains(mac, new ByteComparer()))
                {

                    for(int i = 0; i<AllECMLists.Count; i++)
                    {
                        ObservableCollection<byte[]> array = AllECMLists[i]; 
                        if(array.Contains(mac, new ByteComparer()))
                        {
                            byte[] item = array.First((o)=>ArraysEqual(o,mac));
                            Application.Current.Dispatcher.Invoke(()=>array.Remove(item));
                            break; 
                        }
                    }

                    Application.Current.Dispatcher.Invoke(()=>ECMs.Add(mac)); 
                }
            }
        }

        public void Remove(byte[] mac)
        {
            if (ECMs.Contains(mac, new ByteComparer()))
            {
                byte[] item = ECMs.First((o) => ArraysEqual(o, mac));
                Application.Current.Dispatcher.Invoke(() => ECMs.Remove(item));
            }
        }

        ~ECMList()
        {
            lock (AllECMLists)
            {
                AllECMLists.Remove(ECMs);
            }
        }

        static ECMList()
        {
            AllECMLists = new List<ObservableCollection<byte[]>>(); 
        }

        public static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false; 
            }
            return true;
        }

    }
}
