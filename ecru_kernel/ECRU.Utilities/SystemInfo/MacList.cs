using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.Utilities
{
    public class MacList
    {
        ArrayList mackList = new ArrayList();
        readonly object Lock = new object(); 
        public void Add(byte[] mac)
        {
            try
            {
                lock (Lock)
                {
                    int index = 0;
                    if (!HasElement(out index, mac))
                    {
                        mackList.Add(mac);
                        if (MacAdded != null)
                            MacAdded(mac);
                    }
                }
            }
            catch
            {
                
            }
        }

        public void Remove(byte[] mac)
        {
            try
            {
                lock (Lock)
                {
                    for (int i = 0; i < mackList.Count; i++)
                    {
                        int index = 0;
                        if (HasElement(out index, mac))
                        {
                            if (MacStartRemoved != null)
                                MacStartRemoved(mac);
                            mackList.RemoveAt(index);
                            if (MacRemoved != null)
                                MacRemoved(mac);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        bool HasElement(out int index,byte[] mac)
        {
            index = 0; 
            for (int i = 0; i < mackList.Count; i++)
            {
                byte[] item = (byte[])mackList[i];
                if(item.ByteArrayCompare(mac))
                {
                    index = i; 
                    return true; 
                }
            }

            return false;
        }

        public bool Contains(byte[] mac)
        {
            lock (Lock)
            {
                int index = 0;
                return HasElement(out index, mac);
            }
        }

        public void Clear()
        {
            lock(Lock)
            {
                while(mackList.Count >0)
                {
                    byte[] mac = (byte[])mackList[0];
                    Remove(mac);
                }

            }
        }
            
        public event MacListChange MacAdded;

        public event MacListChange MacRemoved;
        public event MacListChange MacStartRemoved;
    }

    public delegate void MacListChange(byte[] mac); 
}
