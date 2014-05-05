using System;
using System.Collections;
using System.Threading;
using ECRU.BLEController.Packets;
using ECRU.BLEController.Util;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.BLEController
{
    internal class DeviceInfoFactory : IDisposable
    {
        public enum Status_t
        {
            Done,
            Error,
            GotService,
            GotDescriptior,
            GotCharacteristic,
            ReadOut,
            Init
        };

        private readonly object Lock = new object();
        private ArrayList ContainorList = new ArrayList();
        private PacketManager packetmanager;

        public DeviceInfoFactory(PacketManager packetmanager)
        {
            this.packetmanager = packetmanager;
            packetmanager.Subscrib(CommandType.ServiceEvent, RecivedPacketEvent);
            packetmanager.Subscrib(CommandType.DisconnectEvent, RecivedError);
            packetmanager.Subscrib(CommandType.DataEvent, RecivedDataEvent);
        }

        public void Dispose()
        {
            packetmanager.Unsubscrib(CommandType.ServiceEvent, RecivedPacketEvent);
            packetmanager.Unsubscrib(CommandType.DisconnectEvent, RecivedError);
            packetmanager.Unsubscrib(CommandType.DataEvent, RecivedDataEvent);
            lock (Lock)
            {
                while (ContainorList.Count > 0)
                {
                    Remove((ContainorList[0] as InfoContainor));
                }
            }
        }


        public void GetDeviceInfo(byte[] addr, InfoSearchDone callBack)
        {
            lock (Lock)
            {
                if (HasAddrRegistered(addr))
                    return;

                var item = new InfoContainor(TimeOut);
                item.Info = new DeviceInfo();
                item.Info.Address = addr;
                item.Callback = callBack;
                item.Status = Status_t.Init;

                ContainorList.Add(item);
            }

            var disEvent = new DiscoverEvent();
            disEvent.Type = DiscoverType.Service;
            disEvent.StartHandle = 0x0001;
            disEvent.EndHandle = 0xFFFF;
            disEvent.Address = addr;

            packetmanager.Send(disEvent);
        }


        private void RecivedPacketEvent(IPacket packet)
        {
            var item = packet as ServiceEvent;
            if (item != null && HasAddrRegistered(item.Address))
            {
                switch (item.Type)
                {
                    case DiscoverType.Service:
                        HandlePrimaryService(item);
                        break;
                    case DiscoverType.Characteristic:
                        HandleCharacteristicService(item);
                        break;
                    case DiscoverType.Descriptors:
                        HandleDescriptiorService(item);
                        break;
                }
            }
        }

        private void HandleDescriptiorService(ServiceEvent item)
        {
            ServiceDirRes[] val = item.Services;
            InfoContainor obj = GetInfo(item.Address);
            if (val != null)
            {
                foreach (Service s in obj.Info.Services)
                {
                    foreach (ServiceDirRes t in val)
                    {
                        var pair = t as DescriptorPair;
                        if (pair.handle >= s.StartHandel && pair.handle <= s.EndHandel)
                        {
                            for (int i = 0; i < s.Characteristics.Length; i++)
                            {
                                UInt16 start = s.Characteristics[i].Value.handle;
                                UInt16 end = 0xFFFF;
                                if (i != s.Characteristics.Length - 1)
                                    end = s.Characteristics[i + 1].Value.handle;

                                if (start <= pair.handle && end >= pair.handle)
                                {
                                    Debug.Print("Dec UUID at " + s.Characteristics[i].Value.handle.ToString() + ": " +
                                                pair.UUID + " Handle: " + pair.handle);
                                    if (pair.UUID == Def.DESCRIPTION_UUID)
                                        s.Characteristics[i].Description.handle = pair.handle;
                                    if (pair.UUID == Def.FORMAT_UUID)
                                        s.Characteristics[i].Format.handle = pair.handle;
                                    if (pair.UUID == Def.GUIFORMAT_UUID)
                                        s.Characteristics[i].GUIFormat.handle = pair.handle;
                                    if (pair.UUID == Def.RANGE_UUID)
                                        s.Characteristics[i].Range.handle = pair.handle;
                                    if (pair.UUID == Def.SUPSCRIPTIONOPTION_UUID)
                                        s.Characteristics[i].Subscription.handle = pair.handle;
                                }
                            }
                        }
                    }
                }

                if (obj.Info.isCompleted())
                {
                    obj.Status = Status_t.Done;
                    Remove(obj);
                    if (obj.Callback != null)
                        obj.Callback(obj.Status, obj.Info);
                }
            }
            obj.Status = Status_t.GotService;
        }

        private void HandleCharacteristicService(ServiceEvent item)
        {
            ServiceDirRes[] val = item.Services;
            InfoContainor obj = GetInfo(item.Address);
            if (val != null)
            {
                foreach (ServiceDirRes i in val)
                {
                    var p = i as CharacteristicPair;
                    Debug.Print("UUID: " + p.UUID.ToString() + " handle: " + p.handle);
                    if (p.UUID == Def.UPDATETIME_CHARA_UUID)
                        obj.Info.TimeHandel = p.handle;
                    if (p.UUID == Def.SYSTEMID_CHARA_UUID)
                        obj.Info.PassCodeHandel = p.handle;
                    if (p.UUID == Def.PRIMSERVICE_NAME_UUID)
                        obj.Info.Name.handle = p.handle;
                    if (p.UUID == Def.MODEL_NUMBER_UUID)
                        obj.Info.Model.handle = p.handle;
                    if (p.UUID == Def.SERIAL_NUMBER_UUID)
                        obj.Info.Serial.handle = p.handle;
                    if (p.UUID == Def.MANUFACTURER_NAME_UUID)
                        obj.Info.Manufacture.handle = p.handle;
                }


                foreach (Service s in obj.Info.Services)
                {
                    int count = CountHandels(val, Def.GENERIC_VALUE_UUID, s.StartHandel, s.EndHandel);
                    s.Characteristics = new Characteristic[count];

                    if (count != 0)
                    {
                        int index = 0;

                        foreach (ServiceDirRes i in val)
                        {
                            var p = i as CharacteristicPair;
                            if (p.UUID == Def.GENERIC_VALUE_UUID && p.handle >= s.StartHandel && p.handle <= s.EndHandel)
                            {
                                s.Characteristics[index] = new Characteristic();
                                s.Characteristics[index].Value.handle = p.handle;
                                s.Characteristics[index].Value.ReadWriteProps = p.ReadWriteProp;
                                index++;
                            }

                            if (p.UUID == Def.EC_DESCRIPTION_UUID && p.handle >= s.StartHandel &&
                                p.handle <= s.EndHandel)
                            {
                                s.Description.handle = p.handle;
                            }

                            if (p.UUID == Def.UPDATE_UUID && p.handle >= s.StartHandel && p.handle <= s.EndHandel)
                            {
                                s.UpdateHandel = p.handle;
                            }
                        }
                    }
                }
            }
            obj.Status = Status_t.GotCharacteristic;
        }

        private void HandlePrimaryService(ServiceEvent item)
        {
            ServiceDirRes[] val = item.Services;
            InfoContainor obj = GetInfo(item.Address);
            if (val != null)
            {
                int countEvent = CountHandels(val, Def.ECSERVICE_UUID);
                obj.Info.Services = new Service[countEvent];
                int index = 0;

                if (countEvent == 0)
                {
                    Remove(obj);
                    if (obj.Callback != null)
                        obj.Callback(obj.Status, obj.Info);

                    return;
                }

                foreach (ServiceDirRes vser in val)
                {
                    var ser = (PrimaryServicePair) vser;
                    if (Def.IsHandleUUID(ser.UUID))
                    {
                        if (ser.UUID == Def.ECSERVICE_UUID)
                        {
                            obj.Info.Services[index] = new Service();
                            obj.Info.Services[index].EndHandel = ser.Endhandle;
                            obj.Info.Services[index].StartHandel = ser.handle;
                            index++;
                        }

                        var discover = new DiscoverEvent();
                        discover.Address = obj.Info.Address;
                        discover.EndHandle = ser.Endhandle;
                        discover.StartHandle = ser.handle;
                        discover.Type = DiscoverType.Characteristic;
                        packetmanager.Send(discover);
                        if (ser.UUID == Def.ECSERVICE_UUID)
                        {
                            discover.Type = DiscoverType.Descriptors;
                            packetmanager.Send(discover);
                        }
                    }
                }
            }
            obj.Status = Status_t.GotService;
        }

        private static int CountHandels(ServiceDirRes[] items, UInt16 UUID, UInt16 startHandle = 0x0001,
            UInt16 endHandle = 0xffff)
        {
            int count = 0;
            foreach (ServiceDirRes i in items)
            {
                if (i.UUID == UUID && i.handle >= startHandle && i.handle <= endHandle)
                    count++;
            }
            return count;
        }


        private void RecivedError(IPacket packet)
        {
            var disPack = packet as DisconnectEvent;
            if (disPack != null && HasAddrRegistered(disPack.Address))
            {
                InfoContainor obj = GetInfo(disPack.Address);
                obj.Status = Status_t.Error;
                Remove(obj);
                if (obj.Callback != null)
                    obj.Callback(obj.Status, obj.Info);
            }
        }

        private void Remove(InfoContainor obj)
        {
            lock (Lock)
            {
                obj.timeout.Dispose();
                ContainorList.Remove(obj);
            }
        }

        private bool HasAddrRegistered(byte[] addr)
        {
            lock (Lock)
            {
                for (int i = 0; i < ContainorList.Count; i++)
                {
                    if (((InfoContainor) ContainorList[i]).Info.Address.ByteArrayCompare(addr))
                        return true;
                }
            }
            return false;
        }

        private InfoContainor GetInfo(byte[] addr)
        {
            lock (Lock)
            {
                for (int i = 0; i < ContainorList.Count; i++)
                {
                    if (((InfoContainor) ContainorList[i]).Info.Address.ByteArrayCompare(addr))
                        return ((InfoContainor) ContainorList[i]);
                }
            }
            return null;
        }

        // Read All Data 

        public void DoFullRead(DeviceInfo infofile, InfoSearchDone callback)
        {
            var file = new InfoContainor(TimeOut);
            file.Info = infofile;
            file.Callback = callback;
            file.Status = Status_t.ReadOut;
            file.ReadIndex = 0;
            lock (Lock)
            {
                ContainorList.Add(file);
            }

            var Read = new ReadEvent();
            Read.Address = infofile.Address;

            Read.handel = FindPair(file.ReadIndex, file.Info).handle;
            packetmanager.Send(Read);
        }

        private void RecivedDataEvent(IPacket packet)
        {
            var data = packet as DataEvent;
            if (data != null)
            {
                InfoContainor obj = GetInfo(data.Address);
                if (obj != null)
                {
                    Debug.Print("Read: " + data.Handel.ToString());
                    IHandleValue item = FindPair(data.Handel, obj.Info);
                    if (item != null)
                        item.Value = data.Value;

                    while (true)
                    {
                        obj.ReadIndex = obj.ReadIndex + 1;
                        IHandleValue nextitem = FindPair(obj.ReadIndex, obj.Info);


                        if (nextitem == null)
                        {
                            obj.Status = Status_t.Done;
                            Remove(obj);
                            if (obj.Callback != null)
                                obj.Callback(obj.Status, obj.Info);
                            break;
                        }
                        else if (nextitem.handle != 0)
                        {
                            var canRead = nextitem as CharacteristicPair;
                            if (canRead != null)
                            {
                                if ((canRead.ReadWriteProp & Def.READPROP) == 0)
                                    continue;
                            }
                            Debug.Print("NextRead: " + nextitem.handle.ToString());
                            var Read = new ReadEvent();
                            Read.Address = obj.Info.Address;
                            Read.handel = nextitem.handle;
                            packetmanager.Send(Read);
                            break;
                        }
                    }
                }
            }
        }

        private IHandleValue FindPair(ushort handle, DeviceInfo deviceInfo)
        {
            var pool = new IHandleValue[] {deviceInfo.Manufacture, deviceInfo.Model, deviceInfo.Name, deviceInfo.Serial};
            foreach (IHandleValue p in pool)
            {
                if (p.handle == handle)
                    return p;
            }

            foreach (Service s in deviceInfo.Services)
            {
                if (s.Description.handle == handle)
                    return s.Description;

                foreach (Characteristic c in s.Characteristics)
                {
                    pool = new IHandleValue[] {c.Description, c.Format, c.GUIFormat, c.Range, c.Subscription, c.Value};
                    foreach (IHandleValue p in pool)
                    {
                        if (p.handle == handle)
                            return p;
                    }
                }
            }

            return null;
        }

        private IHandleValue FindPair(int index, DeviceInfo deviceInfo)
        {
            int currentIndex = 0;
            var pool = new IHandleValue[] {deviceInfo.Manufacture, deviceInfo.Model, deviceInfo.Name, deviceInfo.Serial};
            foreach (IHandleValue p in pool)
            {
                if (currentIndex == index)
                    return p;
                Debug.Print("At Index: " + currentIndex.ToString() + "handel: " + p.handle.ToString());
                currentIndex++;
            }

            foreach (Service s in deviceInfo.Services)
            {
                if (currentIndex == index)
                    return s.Description;

                Debug.Print("At Index: " + currentIndex.ToString() + "handel: " + s.Description.handle.ToString());
                currentIndex++;

                foreach (Characteristic c in s.Characteristics)
                {
                    pool = new IHandleValue[] {c.Description, c.Format, c.GUIFormat, c.Range, c.Subscription, c.Value};
                    foreach (IHandleValue p in pool)
                    {
                        if (currentIndex == index)
                            return p;
                        Debug.Print("At Index: " + currentIndex.ToString() + "handel: " + p.handle.ToString());
                        currentIndex++;
                    }
                }
            }

            return null;
        }

        private void TimeOut(object state)
        {
            var obj = state as InfoContainor;
            Remove(obj);
            obj.Callback(Status_t.Error, obj.Info);
        }

        private class InfoContainor
        {
            public InfoContainor(TimerCallback timeoutCall)
            {
                CommandStarted = DateTime.Now;
                timeout = new Timer(timeoutCall, this, 60000, Timeout.Infinite); // 5 min timeout. 
            }

            public Timer timeout { get; private set; }

            public DeviceInfo Info { get; set; }

            public Status_t Status { get; set; }

            public InfoSearchDone Callback { get; set; }

            public int ReadIndex { get; set; }

            public DateTime CommandStarted { get; private set; }
        }
    }

    internal delegate void InfoSearchDone(DeviceInfoFactory.Status_t status, DeviceInfo item);
}