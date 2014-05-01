using System;
using System.Net.Sockets;
using ECRU.Utilities;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
using Json.NETMF;
using Microsoft.SPOT;

namespace ECRU.netd.MacSync
{
    public static class MacSync
    {

        public static void RequestDevices(object message)
        {
            var msg = message as ConnectionRequestMessage;

            if (msg != null)
            {
                if (msg.connectionType == "RequestDevices")
                {
                    using (var socket = msg.GetSocket())
                    {
                        //fetch mac hirachy and transform to json

                        var sysMac = SystemInfo.SystemMAC;
                        var connectedDevices = SystemInfo.ConnectedDevices.GetElements();

                        var obj = new Utilities.DeviceMacList();

                        obj.mac = SystemInfo.SystemMAC.ToHex();

                        foreach (byte[] device in connectedDevices)
                        {
                            obj.Devices.Add(device.ToHex());
                        }

                        //var result = "{mac: \"" + sysMac.ToHex() + "\", devices: [";

                        //if (connectedDevices.Length > 1)
                        //{
                        //    foreach (byte[] device in connectedDevices)
                        //    {
                        //        result += "\"" + device.ToHex() + "\", ";
                        //    }
                        //}
                        //else if (connectedDevices.Length == 1)
                        //{
                        //    result += "\"" + ((byte[])connectedDevices.GetValue(0)).ToHex() + "\"";
                        //}

                        //result += "] }";

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


