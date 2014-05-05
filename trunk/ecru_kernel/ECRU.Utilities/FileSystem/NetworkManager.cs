using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using ECRU.Utilities.HelpFunction;
using Microsoft.SPOT;

namespace ECRU.Utilities
{
    public class NetworkManager : IDisposable
    {
        public const string connectionRQType = "filemanagerRq";

        private const string InfoFolderName = "Info";
        private const string FileFolderName = "Files";
        private readonly object BusyLock = new object();
        private readonly object ChangeLock = new object();

        private readonly LocalManager InfoManager;
        private readonly Hashtable MutexCollection = new Hashtable();
        private readonly LocalManager NetFileManager;
        private CordinatorRole cordinator;
        private byte[] cordinatorAddrs;

        public NetworkManager(string path)
        {
            isOnline = false;

            InfoManager = new LocalManager(path + "\\" + InfoFolderName);
            NetFileManager = new LocalManager(path + "\\" + FileFolderName);


            EventBus.Subscribe(typeof (NetworkStatusMessage), netStateChanged);
            EventBus.Subscribe(typeof (ConnectionRequestMessage), connectionEstablishedEvent);
        }

        #region NetStateChanged

        private void StartNetShare()
        {
            if (SystemInfo.SystemMAC != null)
            {
                // Get device with lowest Mac
                cordinatorAddrs = SystemInfo.ConnectionOverview.GetSortedMasters()[0].FromHex();
                Debug.Print("CORDINATOR LOAD : " + cordinatorAddrs.ToHex());
                if (cordinatorAddrs.ByteArrayCompare(SystemInfo.SystemMAC))
                {
                    //I am cordinator 
                    cordinator = new CordinatorRole(InfoManager, NetFileManager);
                }
            }
            isOnline = true;
        }

        private void StopNetShare()
        {
            isOnline = false;
            MutexClear();
            if (cordinator != null)
            {
                cordinator.Dispose();
                cordinator = null;
            }
        }

        private void netStateChanged(object msg)
        {
            lock (ChangeLock)
            {
                var changed = msg as NetworkStatusMessage;
                if (changed != null)
                {
                    Debug.Print("is in Sync: " + changed.isinsync);
                    Debug.Print("NetStage: " + changed.NetState);
                    if (isOnline && changed.isinsync == false)
                    {
                        StopNetShare();
                    }
                    if (isOnline && changed.isinsync)
                    {
                        StopNetShare();
                        StartNetShare();
                    }
                    if (changed.isinsync && isOnline == false)
                    {
                        StartNetShare();
                    }
                }
            }
        }

        #endregion

        public bool isOnline { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //–--------------events ------------------―


        private void connectionEstablishedEvent(object msg)
        {
            var con = msg as ConnectionRequestMessage;

            if (con != null)
            {
                if (con.connectionType == connectionRQType)
                {
                    Socket connection = con.GetSocket();
                    if (connection != null)
                    {
                        HandleNewConnection(connection);
                    }
                }
            }
        }

        private void HandleNewConnection(Socket socket)
        {
            try
            {
                bool waitingForData = true;

                while (waitingForData)
                {
                    waitingForData = !socket.Poll(10, SelectMode.SelectRead) && !socket.Poll(10, SelectMode.SelectError);

                    if (socket.Available > 0)
                    {
                        var data = new byte[socket.Available];
                        socket.Receive(data);

                        byte cmd = data[0];
                        byte[] pack = data.GetPart(1, data.Length - 1);

                        switch (cmd)
                        {
                            case 0x03: //Update File
                                UpdateFileCommand(pack);
                                break;

                            case 0x05: //Get File
                            {
                                string file = pack.GetString();

                                FileBase b = NetFileManager.GetReadOnlyFile(file);
                                string infoname = Path.GetFileNameWithoutExtension(file) + ".info";
                                FileBase i = InfoManager.GetReadOnlyFile(infoname);

                                if (i != null && b != null)
                                {
                                    var filelength = new byte[2];
                                    filelength[0] = (byte) (b.Data.Length >> 8);
                                    filelength[1] = (byte) (b.Data.Length);
                                    var infolength = new byte[2];
                                    infolength[0] = (byte) (i.Data.Length >> 8);
                                    infolength[1] = (byte) (i.Data.Length);

                                    socket.Send(filelength.Add(b.Data).Add(infolength).Add(i.Data));
                                }
                            }
                                break;

                            case 0x07: //File States
                            {
                                lock (BusyLock)
                                {
                                    FileInfo[] files = NetFileManager.GetFiles();

                                    foreach (FileInfo f in files)
                                    {
                                        string infoname = Path.GetFileNameWithoutExtension(f.Name) + ".info";
                                        string filename = f.Name;

                                        FileBase b = InfoManager.GetReadOnlyFile(infoname);
                                        if (b != null)
                                        {
                                            var infof = new InfoFile(b);

                                            byte[] packet = infof.Version.ToByte();
                                            packet = packet.Add(filename.StringToBytes());
                                            socket.Send(packet);
                                        }
                                    }

                                    socket.Close();
                                }
                            }
                                break;
                        }
                    }
                }
            }
            finally
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }
        }

        private void UpdateFileCommand(byte[] pack)
        {
            int pathLength = (pack[0] << 8) + pack[1];
            byte[] filePath = pack.GetPart(2, pathLength);
            string path = filePath.GetString();

            int fileLength = (pack[2 + pathLength] << 8) + pack[3 + pathLength];
            byte[] file = pack.GetPart(3 + pathLength + 1, fileLength);

            int infoLength = (pack[3 + pathLength + 1 + fileLength] << 8) + pack[3 + pathLength + 2 + fileLength];
            byte[] info = pack.GetPart(3 + pathLength + 3 + fileLength, infoLength);

            lock (BusyLock)
            {
                FileBase File_file = null;
                FileBase File_info = null;

                if (NetFileManager.FileExists(path))
                {
                    File_file = NetFileManager.GetFile(path);
                }
                else
                {
                    File_file = NetFileManager.CreateFile(path);
                }

                if (InfoManager.FileExists(Path.GetFileNameWithoutExtension(path) + ".info"))
                {
                    File_info = InfoManager.GetFile(Path.GetFileNameWithoutExtension(path) + ".info");
                }
                else
                {
                    File_info = InfoManager.CreateFile(Path.GetFileNameWithoutExtension(path) + ".info");
                }

                if (File_file != null && File_info != null)
                {
                    File_file.Data = file;
                    File_info.Data = info;
                }

                if (File_info != null)
                    File_info.Close();
                if (File_file != null)
                    File_file.Close();
            }
        }

        private void ConnectionRq(object msg)
        {
        }

        //--------------------------------------------------------------------

        private bool isMutexFree(string path)
        {
            lock (MutexCollection)
            {
                if (MutexCollection.Contains(path))
                {
                    var s = MutexCollection[path] as Socket;
                    if (SocketConnected(s))
                        return false;
                    freeMutex(path, s);
                    return true;
                }
            }

            return true;
        }

        private static bool SocketConnected(Socket socket)
        {
            if (socket != null)
            {
                bool ret = !socket.Poll(1, SelectMode.SelectRead) && !socket.Poll(1, SelectMode.SelectError);
                try
                {
                    ret = socket.Available == 0 ? ret : ret;
                }
                catch
                {
                    ret = false;
                    if (socket != null)
                        socket.Close();
                }
                return ret;
            }
            return false;
        }

        private bool getMutex(string path, Socket s)
        {
            lock (MutexCollection)
            {
                if (isMutexFree(path))
                {
                    MutexCollection.Add(path, s);
                    return true;
                }
                return false;
            }
        }

        private bool freeMutex(string path, Socket socket)
        {
            lock (MutexCollection)
            {
                if (!isMutexFree(path))
                {
                    var s = MutexCollection[path] as Socket;
                    if (s == socket)
                    {
                        if (s != null)
                            s.Close();

                        MutexCollection.Remove(path);
                        return true;
                    }
                }
                return false;
            }
        }

        private void MutexClear()
        {
            while (MutexCollection.Count > 0)
            {
                IEnumerator keys = MutexCollection.Keys.GetEnumerator();
                var path = keys.Current as string;
                var s = MutexCollection[path] as Socket;

                if (s != null)
                    s.Close();

                MutexCollection.Remove(path);
            }
        }


        //--------------------------------------------------------------------

        public bool FileExists(string path)
        {
            return NetFileManager.FileExists(path);
        }

        public FileBase GetFile(string path)
        {
            if (isMutexFree(path) && isOnline)
            {
                //GetMutex 
                //Create Close Method; 
                // return file; 
            }
            return null;
        }

        public FileBase GetReadOnlyFile(string path)
        {
            return NetFileManager.GetReadOnlyFile(path);
        }

        public bool DeleteFile(string path)
        {
            if (isOnline)
            {
                //Get Mutex 
                //Send Deleat File Command 
                //Close connection
            }
            return false;
        }


        /*
        public void CloseFile(FileBase localfile)
        {
            if (isOnline)
            {
                //Send Update.
                //Close Connection 
            }
        }
        */

        //--------------------------------------------------------------------
    }
}