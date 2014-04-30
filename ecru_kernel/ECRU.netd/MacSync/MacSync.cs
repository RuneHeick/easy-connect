using System;
using ECRU.Utilities;
using ECRU.Utilities.EventBus.Events;
using ECRU.Utilities.HelpFunction;
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

                        var result = "{mac: \"" + sysMac.ToHex() + "\", devices: [";

                        if (connectedDevices.Length > 1)
                        {
                            foreach (byte[] device in connectedDevices)
                            {
                                result += "\"" + device.ToHex() + "\", ";
                            }
                        }
                        else if (connectedDevices.Length == 1)
                        {
                            result += "\"" + ((byte[])connectedDevices.GetValue(0)).ToHex() + "\"";
                        }

                        result += "] }";

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
                if (msg.connectionType == "WhoHasDevice")
                {
                    using (var socket = msg.GetSocket())
                    {
                        //We receive a connection where a roomunit wants to tell us that it has the device we asked about

                        var sysMac = SystemInfo.SystemMAC;
                        var connectedDevices = SystemInfo.ConnectedDevices.GetElements();

                        var result = "{mac: \"" + sysMac.ToHex() + "\", devices: [";

                        if (connectedDevices.Length > 1)
                        {
                            foreach (byte[] device in connectedDevices)
                            {
                                result += "\"" + device.ToHex() + "\", ";
                            }
                        }
                        else if (connectedDevices.Length == 1)
                        {
                            result += "\"" + ((byte[])connectedDevices.GetValue(0)).ToHex() + "\"";
                        }

                        result += "] }";

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

    }
}
