using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.Utilities
{
    public class MacHierarchy
    {
        private readonly object Lock = new object();
        private readonly Hashtable data = new Hashtable();

        public void Add(byte[] master, byte[] unit)
        {
            if (unit != null)
            {
                lock (Lock)
                {
                    string masterMac = master.ToHex();
                    Remove(unit);
                    if (data.Contains(masterMac))
                    {
                        var list = (MacList) data[masterMac];
                        list.Add(unit);
                    }
                    else
                    {
                        var list = new MacList();
                        list.Add(unit);
                        data.Add(masterMac, list);
                        list.MacAdded += (o => list_MacChanged(o, UnitAdded));
                        list.MacStartRemoved += (o => list_MacChanged(o, UnitRemoved));
                    }
                    if (UnitAdded != null)
                        UnitAdded(master, unit);
                }
            }
        }

        private void list_MacChanged(byte[] mac, MacHierarchyChange call)
        {
            lock (Lock)
            {
                if (call != null)
                {
                    ICollection keyCollection = data.Keys;
                    var keys = new string[keyCollection.Count];
                    keyCollection.CopyTo(keys, 0);

                    for (int i = 0; i < data.Count; i++)
                    {
                        var maclist = (MacList) data[keys[i]];
                        if (maclist.Contains(mac))
                        {
                            call(keys[i].FromHex(), mac);
                            return;
                        }
                    }
                }
            }
        }

        public void Add(byte[] master)
        {
            lock (Lock)
            {
                string masterMac = master.ToHex();
                if (data.Contains(masterMac))
                {
                }
                else
                {
                    var list = new MacList();
                    data.Add(masterMac, list);
                }
                if (UnitAdded != null)
                    UnitAdded(master, null);
            }
        }

        public void Remove(byte[] master, byte[] unit)
        {
            string masterMac = master.ToHex();
            if (data.Contains(masterMac))
            {
                var list = (MacList) data[masterMac];
                list.Remove(unit);
                if (UnitRemoved != null)
                    UnitRemoved(master, unit);
            }
        }

        public void Remove(byte[] unit)
        {
            lock (Lock)
            {
                ICollection keyCollection = data.Keys;
                var keys = new string[keyCollection.Count];
                keyCollection.CopyTo(keys, 0);

                for (int i = 0; i < data.Count; i++)
                {
                    var maclist = (MacList) data[keys[i]];

                    string k = keys[i];
                    if (k.FromHex().ByteArrayCompare(unit))
                    {
                        maclist.Clear();
                        data.Remove(k);
                        break;
                    }

                    if (maclist.Contains(unit))
                    {
                        maclist.Remove(unit); // Event Will triger remove event. 
                    }
                }
            }
        }

        public string[] GetSortedMasters()
        {
            lock (Lock)
            {
                var units = new string[data.Count];
                int index = 0;
                foreach (object k in data.Keys)
                {
                    units[index++] = (string) k;
                }
                return units.Quicksort(0, units.Length - 1);
            }
        }

        public MacList GetDecices(byte[] Master)
        {
            lock (Lock)
            {
                string masterMac = Master.ToHex();
                if (data.Contains(masterMac))
                {
                    var list = (MacList) data[masterMac];
                    return list;
                }
            }
            return null;
        }


        public event MacHierarchyChange UnitRemoved;
        public event MacHierarchyChange UnitAdded;
    }

    public delegate void MacHierarchyChange(byte[] master, byte[] unit);
}