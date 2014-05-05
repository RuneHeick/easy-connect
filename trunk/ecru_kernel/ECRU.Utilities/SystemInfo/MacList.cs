using System;
using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.Utilities
{
    public class MacList
    {
        private readonly object Lock = new object();
        private readonly ArrayList mackList = new ArrayList();

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

        public Array GetElements()
        {
            var list = new Array[mackList.Count];

            mackList.CopyTo(list);

            return list;
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

        private bool HasElement(out int index, byte[] mac)
        {
            index = 0;
            for (int i = 0; i < mackList.Count; i++)
            {
                var item = (byte[]) mackList[i];
                if (item.ByteArrayCompare(mac))
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
            lock (Lock)
            {
                while (mackList.Count > 0)
                {
                    var mac = (byte[]) mackList[0];
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