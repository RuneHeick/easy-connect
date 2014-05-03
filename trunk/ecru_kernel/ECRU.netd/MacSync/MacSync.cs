using System;
using System.Net.Sockets;
using ECRU.Utilities;
using ECRU.Utilities.EventBus;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
using Json.NETMF;
using Microsoft.SPOT;

namespace ECRU.netd.MacSync
{
    public static class MacSync
    {

        public static void GotDevice(byte[] unit)
        {
            var msg = SystemInfo.SystemMAC;
            msg = msg.Add(unit);
            EventBus.Publish(new SendBroadcastMessage{BroadcastType = new byte[] { 3 }, Message = msg});
        }

        public static void GotDeviceNetworkEvent(object message)
        {
            var msg = message as RecivedBroadcastMessage;

            if (msg == null) return;

            if (!msg.MessageType.ByteArrayCompare(new byte[] { 3 })) return;

            var roomunit = msg.Message.GetPart(0, 6);
            var ecm = msg.Message.GetPart(6, 6);
            
            if (roomunit != null && ecm != null)
            {
                SystemInfo.ConnectionOverview.Add(roomunit, ecm);
            }
        }

        public static void LostDevice(byte[] unit)
        {
            var msg = SystemInfo.SystemMAC;
            msg = msg.Add(unit);
            EventBus.Publish(new SendBroadcastMessage { BroadcastType = new byte[] { 4 }, Message = msg });
        }

        public static void LostDeviceNetworkEvent(object message)
        {
            var msg = message as RecivedBroadcastMessage;

            if (msg == null) return;

            if (!msg.MessageType.ByteArrayCompare(new byte[] { 4 })) return;

            var roomunit = msg.Message.GetPart(0, 6);
            var ecm = msg.Message.GetPart(6, 6);

            if (roomunit != null && ecm != null)
            {
                SystemInfo.ConnectionOverview.Remove(roomunit, ecm);
            }
        }



        public static void RequestDevices(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg == null) return;

            if (msg.connectionType != "RequestDevices") return;
            var socket = msg.GetSocket();

            if (socket != null)
            {
                using (socket)
                {
                    //fetch mac hirachy and transform to json

                    var sysMac = SystemInfo.SystemMAC;
                    var connectedDevices = SystemInfo.ConnectedDevices.GetElements();

                    var obj = new Utilities.DeviceMacList();

                    obj.mac = SystemInfo.SystemMAC.ToHex();
                    obj.Name = SystemInfo.Name;

                    foreach (byte[] device in connectedDevices)
                    {
                        obj.Devices.Add(device.ToHex());
                    }

                    var result = JsonSerializer.SerializeObject(obj);
                    Debug.Print(result);

                    try
                    {
                        socket.Send(result.StringToBytes());
                    }
                    catch (Exception exception)
                    {
                        Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                    }
                    finally
                    {
                        if (socket != null)
                        {
                            socket.Close();
                        }
                    }
                }
            }
        }

        public static void WhoHasDevice(object message)
        {

            var msg = message as ConnectionRequestMessage;

            if (msg != null)
            {
                if (msg.connectionType == "HasDevice")
                {
                    using (var socket = msg.GetSocket())
                    {
                        //We receive a connection where a roomunit wants to tell us that it has the device we asked about

                        try
                        {
                            var waitingForData = true;

                            while (waitingForData)
                            {
                                waitingForData = !socket.Poll(10, SelectMode.SelectRead) && !socket.Poll(10, SelectMode.SelectError);

                                if (socket.Available > 0)
                                {
                                    var availableBytes = socket.Available;

                                    var buffer = new byte[availableBytes];

                                    var bytesReceived = socket.Receive(buffer);

                                    if (bytesReceived == availableBytes)
                                    {
                                        Debug.Print("Got device: " + buffer.ToHex());
                                    }

                                    SystemInfo.ConnectionOverview.Add(buffer.GetPart(0,6), buffer.GetPart(6,6));
                                }
                            }

                        }
                        catch (Exception exception)
                        {
                            Debug.Print("Network error: " + exception.Message + " stacktrace: " + exception.StackTrace);
                        }
                        finally
                        {
                            if (socket != null)
                            {
                                socket.Close();
                            }
                        }


                    }

                }
            }
        }

    }
}


