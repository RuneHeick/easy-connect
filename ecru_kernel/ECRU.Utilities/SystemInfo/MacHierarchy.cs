using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.Utilities
{
    public class MacHierarchy
    {
        Hashtable data = new Hashtable(); 
        readonly object Lock = new object(); 

        public void Add(byte[] master, byte[] unit )
        {
            if (unit != null)
            {
                lock (Lock)
                {
                    string masterMac = master.ToHex();
                    Remove(unit);
                    if (data.Contains(masterMac))
                    {
                        MacList list = (MacList)data[masterMac];
                        list.Add(unit);
                    }
                    else
                    {
                        MacList list = new MacList();
                        list.Add(unit);
                        data.Add(masterMac, list);
                        list.MacAdded += ((o) => list_MacChanged(o, UnitAdded));
                        list.MacStartRemoved += ((o) => list_MacChanged(o, UnitRemoved));
                    }
                    if (UnitAdded != null)
                        UnitAdded(master, unit);
                }
            }
        }

        void list_MacChanged(byte[] mac,MacHierarchyChange call )
        {
            lock(Lock)
            {
                if (call != null)
                {
                    ICollection keyCollection = data.Keys;
                    string[] keys = new string[keyCollection.Count];
                    keyCollection.CopyTo(keys, 0);

                    for (int i = 0; i < data.Count; i++)
                    {
                        MacList maclist = (MacList)data[keys[i]];
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
                    MacList list = new MacList();
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
                    MacList list = (MacList)data[masterMac];
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
                    string[] keys = new string[keyCollection.Count];
                    keyCollection.CopyTo(keys, 0);

                    for (int i = 0; i < data.Count; i++)
                    {
                        MacList maclist = (MacList)data[keys[i]];
                        if (maclist.Contains(unit))
                        {
                            maclist.Remove(unit); // Event Will triger remove event. 
                        }
                    }
            }
        }


        public MacList GetDecices(byte[] Master)
        {
            lock (Lock)
            {
                string masterMac = Master.ToHex();
                if (data.Contains(masterMac))
                {
                    MacList list = (MacList)data[masterMac];
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
