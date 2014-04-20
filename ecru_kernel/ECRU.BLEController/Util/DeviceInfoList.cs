using System;
using Microsoft.SPOT;
using System.Collections;
using ECRU.Utilities.HelpFunction;

namespace ECRU.BLEController.Util
{
    public class DeviceInfoList
    {
        ArrayList data = new ArrayList();

        public void Add(string Name, byte[] Addr)
        {
            Add(new DeviceInfo(Name, Addr));
        }

        public void Add(DeviceInfo item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                DeviceInfo testitem = data[i] as DeviceInfo;
                if (testitem.Addr.ByteArrayCompare(item.Addr))
                    return;
            }
            data.Add(item);
        }

        public void Remove(byte[] addr)
        {
            for(int i = 0; i<data.Count;i++)
            {
                DeviceInfo item = data[i] as DeviceInfo;
                if(item != null && item.Addr.ByteArrayCompare(addr))
                {
                    data.RemoveAt(i);
                    break;
                }
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public DeviceInfo this[int i]
        {
            get
            {
                return data[i] as DeviceInfo;
            }
        }


        public class DeviceInfo
        {
            public string Name { get; private set; }
            public byte[] Addr { get; private set;  }

            public DeviceInfo(string Name,byte[] Addr)
            {
                this.Name = Name;
                this.Addr = Addr; 
            }

        }

    }
}
